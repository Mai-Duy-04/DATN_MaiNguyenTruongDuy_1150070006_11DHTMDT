using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TourWebApp.Data.Models;
using TourWebApp.Helpers;
using TourWebApp.Services;

namespace TourWebApp.Controllers;

// ============================================================
// API REQUEST
// ============================================================

public sealed class TripiChatApiRequest
{
    public string Message { get; set; } = "";

    public List<TripiChatMessage> History { get; set; } = new();

    public int? IdTourDangXem { get; set; }

    public TripiTravelPreferences? Preferences { get; set; }
}

// ============================================================
// TRIPI CHAT CONTROLLER
// ============================================================

[ApiController]
[Route("api/tripi-chat")]
public sealed class TripiChatController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TripiChatController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly IMatchingScoreService _matchingService;
    private readonly ILoggerFactory _loggerFactory;
    private const int MaxMessageLength = 1000;
    private const int MaxHistoryMessages = 12;
    private const int MaxRequestCharacters = 16000;

    public TripiChatController(
    IConfiguration configuration,
    ILogger<TripiChatController> logger,
    ApplicationDbContext db,
    IMatchingScoreService matchingService,
    ILoggerFactory loggerFactory)
{
    _configuration = configuration;
    _logger = logger;
    _db = db;
    _matchingService = matchingService;
    _loggerFactory = loggerFactory;
}

    // ========================================================
    // 1. NHAN TIN NHAN
    // ========================================================

    [HttpPost("gui-tin-nhan")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(32768)]
    [Consumes("application/json")]
    public async Task<IActionResult> GuiTinNhan(
        [FromBody] TripiChatApiRequest? apiRequest,
        CancellationToken cancellationToken)
    {
        // ====================================================
        // 2. KIEM TRA REQUEST
        // ====================================================

        if (apiRequest == null ||
            string.IsNullOrWhiteSpace(apiRequest.Message))
        {
            return BadRequest(new
            {
                message = "Bạn hãy nhập nội dung muốn hỏi Tripi nhé."
            });
        }

        apiRequest.Message = apiRequest.Message.Trim();

        if (apiRequest.Message.Length > MaxMessageLength)
        {
            return BadRequest(new
            {
                message =
                    "Mỗi tin nhắn không được vượt quá 1000 ký tự."
            });
        }

        apiRequest.History ??= new List<TripiChatMessage>();

        if (apiRequest.History.Count > MaxHistoryMessages)
        {
            return BadRequest(new
            {
                message =
                    "Lịch sử hội thoại quá dài. " +
                    "Vui lòng tải lại trang."
            });
        }

        if (apiRequest.IdTourDangXem.HasValue &&
            apiRequest.IdTourDangXem.Value <= 0)
        {
            return BadRequest(new
            {
                message = "Mã tour đang xem không hợp lệ."
            });
        }

        int totalCharacters = apiRequest.Message.Length;

        foreach (TripiChatMessage item in apiRequest.History)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(item.Content) ||
                item.Content.Length > MaxMessageLength ||
                (item.Role != "user" && item.Role != "model"))
            {
                return BadRequest(new
                {
                    message = "Lịch sử hội thoại không hợp lệ."
                });
            }

            totalCharacters += item.Content.Length;
        }

        if (totalCharacters > MaxRequestCharacters)
        {
            return BadRequest(new
            {
                message =
                    "Nội dung hội thoại vượt quá giới hạn."
            });
        }

        var savedPreferences = apiRequest.Preferences;

        if (savedPreferences != null)
        {
            bool invalidPreferences =
                (savedPreferences.DiemDen?.Length ?? 0) > 200 ||
                (savedPreferences.LoaiTour?.Length ?? 0) > 200 ||
                (savedPreferences.NgayDuKien?.Length ?? 0) > 30 ||
                (savedPreferences.TuNgay?.Length ?? 0) > 30 ||
                (savedPreferences.DenTruocNgay?.Length ?? 0) > 30 ||
                (savedPreferences.SoThich?.Count ?? 0) > 20 ||
                (savedPreferences.SoThich?.Any(x =>
                    string.IsNullOrWhiteSpace(x) ||
                    x.Length > 100) ?? false) ||
                (savedPreferences.NganSach.HasValue &&
                    (savedPreferences.NganSach.Value < 0 ||
                    savedPreferences.NganSach.Value > 1000000000m)) ||
                (savedPreferences.SoNgay.HasValue &&
                    (savedPreferences.SoNgay.Value < 1 ||
                    savedPreferences.SoNgay.Value > 365)) ||
                (savedPreferences.SoKhach.HasValue &&
                    (savedPreferences.SoKhach.Value < 1 ||
                    savedPreferences.SoKhach.Value > 1000)) ||
                (!string.IsNullOrEmpty(savedPreferences.LoaiNganSach) &&
                    savedPreferences.LoaiNganSach != "MoiNguoi" &&
                    savedPreferences.LoaiNganSach != "CaNhom");

            if (!string.IsNullOrWhiteSpace(savedPreferences.NgayDuKien))
            {
                invalidPreferences |=
                    !DateOnly.TryParseExact(
                        savedPreferences.NgayDuKien,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _);
            }

            if (savedPreferences.TuNgay != null ||
                savedPreferences.DenTruocNgay != null)
            {
                bool validRange =
                    DateOnly.TryParseExact(
                        savedPreferences.TuNgay,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var from) &&
                    DateOnly.TryParseExact(
                        savedPreferences.DenTruocNgay,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var to) &&
                    from < to &&
                    to.DayNumber - from.DayNumber <= 366;

                invalidPreferences |= !validRange;
            }

            if (invalidPreferences)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu nhu cầu du lịch không hợp lệ."
                });
            }
        }

        // ====================================================
        // CAP NHAT NGU CANH BANG GEMINI
        // ====================================================

        if (savedPreferences?.TuNgay is string tuNgay &&
            DateOnly.TryParseExact(
                tuNgay,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            try
            {
                var contextGeminiService =
                    new TripiContextGeminiService(
                        _configuration,
                        _loggerFactory.CreateLogger<TripiContextGeminiService>());

                TripiContextUpdate update =
                    await contextGeminiService.AnalyzeUpdateAsync(
                        apiRequest.Message,
                        savedPreferences,
                        apiRequest.History,
                        cancellationToken);

                bool hasChanges = new[]
                {
                    update.DiemDen?.Action,
                    update.SoThich?.Action,
                    update.NganSach?.Action,
                    update.LoaiNganSach?.Action,
                    update.SoNgay?.Action,
                    update.LoaiTour?.Action,
                    update.SoKhach?.Action,
                    update.NgayDuKien?.Action,
                    update.TuNgay?.Action,
                    update.DenTruocNgay?.Action
                }.Any(action =>
                    string.Equals(
                        action,
                        "set",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        action,
                        "clear",
                        StringComparison.OrdinalIgnoreCase));

                if (hasChanges)
                {
                    var contextService =
                        new TripiConversationContextService();

                    TripiTravelPreferences updatedPreferences =
                        contextService.ApplyUpdate(
                            savedPreferences,
                            update);

                    if (!string.IsNullOrWhiteSpace(
                            updatedPreferences.TuNgay) &&
                        DateOnly.TryParseExact(
                            updatedPreferences.TuNgay,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateOnly startDate))
                    {
                        return await LietKeLichTheoThangAsync(
                            startDate.Month,
                            startDate.Year,
                            apiRequest.Message,
                            updatedPreferences,
                            cancellationToken,
                            true);
                    }
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "[Tripi Context] Invalid preference update.");

                return BadRequest(new
                {
                    message =
                        "Tripi chưa xác định được điều kiện thay đổi. " +
                        "Bạn hãy diễn đạt rõ hơn một chút nhé."
                });
            }
            catch (Exception ex) when (
                ex is InvalidOperationException ||
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException)
            {
                _logger.LogWarning(
                    ex,
                    "[Tripi Context] Cannot analyze conversation.");

                return StatusCode(503, new
                {
                    message =
                        "Tripi chưa phân tích được yêu cầu thay đổi. " +
                        "Bạn vui lòng thử lại nhé."
                });
            }
        }

        if (TryReadMonthQuery(
            apiRequest.Message,
            out int thang,
            out int nam))
        {
            return await LietKeLichTheoThangAsync(
                thang,
                nam,
                apiRequest.Message,
                savedPreferences,
                cancellationToken);
        }

        var chatRequest = new TripiChatRequest
        {
            Message = apiRequest.Message,
            History = apiRequest.History,
            Preferences = savedPreferences
        };

        // ====================================================
        // 3. GEMINI PHAN LOAI Y DINH
        // ====================================================
        
        TripiChatResponse chatResult;
        try
        {
            // Yeu cau tim tour ro rang:
            // Khong can goi Gemini lan 1 de phan loai.
            // Gemini lan 2 van phan tich nhu cau thuc te.
                var chatService = new TripiChatService(
                    _configuration);

                chatResult = await chatService.TraLoiAsync(
                    chatRequest,
                    cancellationToken);

            _logger.LogInformation(
                "[Tripi] Intent={Intent}; CanSearch={CanSearch}; " +
                "HistoryCount={HistoryCount}",
                chatResult.Intent,
                chatResult.CanSearchTours,
                chatRequest.History.Count);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(
                ex,
                "[Tripi] Gemini chat request timed out.");

            return StatusCode(504, new
            {
                message =
                    "Tripi phản hồi quá lâu. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "[Tripi] Cannot connect to Gemini chat.");

            return StatusCode(503, new
            {
                message =
                    "Tripi chưa kết nối được với Gemini. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "[Tripi] Gemini chat returned invalid JSON.");

            return StatusCode(502, new
            {
                message =
                    "Tripi chưa xử lý được phản hồi AI. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "[Tripi] Gemini chat failed.");

            return StatusCode(503, new
            {
                message =
                    "Tripi đang gặp sự cố khi tạo câu trả lời. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }

        // ====================================================
        // 4. CAU HOI CHI TIET TOUR -> RAG
        // ====================================================
        if (chatResult.Intent == "recall_preferences")
        {
            TripiTravelPreferences? p = apiRequest.Preferences;

            string reply;

            if (p == null)
            {
                reply =
                    "Mình chưa có điều kiện tìm tour nào được lưu " +
                    "trong cuộc trò chuyện này. " +
                    "Bạn cứ chia sẻ mong muốn du lịch, " +
                    "mình sẽ ghi nhận để hỗ trợ nhé.";
            }
            else
            {
                var details = new List<string>();

                if (!string.IsNullOrWhiteSpace(p.DiemDen))
                {
                    details.Add($"• Điểm đến: {p.DiemDen}");
                }

                if (p.NganSach.HasValue)
                {
                    string scope = p.LoaiNganSach switch
                    {
                        "MoiNguoi" => "/người",
                        "CaNhom" => " cho cả nhóm",
                        _ => ""
                    };

                    details.Add(
                        $"• Ngân sách: {p.NganSach.Value:N0}đ{scope}");
                }

                if (p.SoKhach.HasValue)
                {
                    details.Add(
                        $"• Số khách: {p.SoKhach.Value} người");
                }

                if (!string.IsNullOrWhiteSpace(p.TuNgay) &&
                    !string.IsNullOrWhiteSpace(p.DenTruocNgay))
                {
                    details.Add(
                        $"• Thời gian: từ {p.TuNgay} " +
                        $"đến trước {p.DenTruocNgay}");
                }
                else if (!string.IsNullOrWhiteSpace(p.NgayDuKien))
                {
                    details.Add(
                        $"• Ngày dự kiến: {p.NgayDuKien}");
                }

                if (p.SoNgay.HasValue)
                {
                    details.Add(
                        $"• Thời lượng: {p.SoNgay.Value} ngày");
                }

                if (!string.IsNullOrWhiteSpace(p.LoaiTour))
                {
                    details.Add(
                        $"• Loại tour: {p.LoaiTour}");
                }

                if (p.SoThich?.Count > 0)
                {
                    details.Add(
                        $"• Sở thích: {string.Join(", ", p.SoThich)}");
                }

                reply = details.Count == 0
                    ? "Mình chưa có điều kiện tìm tour cụ thể nào được lưu."
                    : "Mình đang ghi nhớ những mong muốn du lịch sau:\n\n" +
                    string.Join("\n", details) +
                    "\n\nBạn có thể thay đổi bất kỳ điều kiện nào nhé.";
            }

            return Ok(new
            {
                reply,
                intent = "recall_preferences",
                canSearchTours = false,
                preferences = p,
                searchPerformed = false,
                tours = Array.Empty<object>(),
                searchMessage = "",
                activeTourId = apiRequest.IdTourDangXem,
                ragHasEvidence = false,
                ragSources = Array.Empty<object>()
            });
        }

        if (chatResult.Intent == "tour_info")
        {
            return await TraLoiThongTinTourAsync(
                apiRequest,
                cancellationToken);
        }

        // ====================================================
        // 5. TIM TOUR THEO Y DINH
        // ====================================================

        bool shouldSearchTours =
            chatResult.CanSearchTours &&
            (chatResult.Intent == "search_tour" ||
             chatResult.Intent == "refine");

        if (shouldSearchTours)
        {
            return await TimTourTrongChatAsync(
                chatRequest,
                chatResult,
                cancellationToken);
        }

        // ====================================================
        // 6. HOI THOAI THONG THUONG
        // ====================================================

        string generalReply = chatResult.Reply;

        if (chatResult.Intent == "compare_tours")
        {
            generalReply =
                "Mình có thể giúp bạn xem những tiêu chí " +
                "nên so sánh giữa các tour. " +
                "Phần đối chiếu dữ liệu trực tiếp giữa " +
                "nhiều tour hiện chưa được kết nối, " +
                "nên mình chưa muốn đưa ra kết luận " +
                "thiếu căn cứ. Bạn đang cân nhắc " +
                "những tour nào vậy?";
        }

        return Ok(new
        {
            reply = generalReply,
            intent = chatResult.Intent,
            canSearchTours = false,

            preferences = (object?)null,

            searchPerformed = false,
            tours = Array.Empty<object>(),
            searchMessage = "",

            activeTourId = apiRequest.IdTourDangXem,

            ragHasEvidence = false,
            ragSources = Array.Empty<object>()
        });
    }

    // ========================================================
    // 7. RAG - TRA LOI THONG TIN TOUR
    // ========================================================

    private async Task<IActionResult> TraLoiThongTinTourAsync(
        TripiChatApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.IdTourDangXem.HasValue)
        {
            return Ok(new
            {
                reply =
                    "Mình muốn kiểm tra đúng tour mà bạn " +
                    "đang hỏi. Bạn hãy chọn " +
                    "'Hỏi về tour này' ở một hành trình " +
                    "bên cạnh nhé. Sau đó mình sẽ " +
                    "tra cứu thông tin HappyTrip.",

                intent = "tour_info",
                canSearchTours = false,
                preferences = (object?)null,

                searchPerformed = false,
                tours = Array.Empty<object>(),
                searchMessage = "",

                activeTourId = (int?)null,

                ragHasEvidence = false,
                ragSources = Array.Empty<object>()
            });
        }

        int idTour = request.IdTourDangXem.Value;

        try
        {
            var ragService = new TripiRagService(
                _db,
                _configuration);

            TripiRagResult ragResult =
                await ragService.TraLoiVeTourAsync(
                    idTour,
                    request.Message,
                    cancellationToken);

            _logger.LogInformation(
                "[Tripi RAG] TourId={TourId}; " +
                "HasEvidence={HasEvidence}; SourceCount={SourceCount}",
                idTour,
                ragResult.HasEvidence,
                ragResult.Sources.Count);

            return Ok(new
            {
                reply = ragResult.Reply,
                intent = "tour_info",
                canSearchTours = false,

                preferences = (object?)null,

                searchPerformed = false,
                tours = Array.Empty<object>(),
                searchMessage = "",

                activeTourId = idTour,

                ragHasEvidence = ragResult.HasEvidence,

                ragSources = ragResult.Sources.Select(s => new
                {
                    idTaiLieu = s.IdTaiLieu,
                    idTour = s.IdTour,
                    tieuDe = s.TieuDe,
                    phienBan = s.PhienBan
                }).ToList()
            });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is HttpRequestException ||
            ex is TaskCanceledException ||
            ex is JsonException)
        {
            _logger.LogWarning(
                ex,
                "[Tripi RAG] Failed for TourId={TourId}",
                idTour);

            return Ok(new
            {
                reply =
                    "Mình chưa truy xuất được tài liệu " +
                    "của tour này lúc này. " +
                    "Bạn có thể xem trang chi tiết tour " +
                    "hoặc hỏi lại mình sau nhé.",

                intent = "tour_info",
                canSearchTours = false,

                preferences = (object?)null,

                searchPerformed = false,
                tours = Array.Empty<object>(),
                searchMessage = "",

                activeTourId = idTour,

                ragHasEvidence = false,
                ragSources = Array.Empty<object>()
            });
        }
    }

    // ========================================================
    // 8. MATCHING SCORE - TIM TOUR
    // ========================================================

    private async Task<IActionResult> TimTourTrongChatAsync(
        TripiChatRequest request,
        TripiChatResponse chatResult,
        CancellationToken cancellationToken)
    {
        try
        {
            var matchingService = new TripiChatMatchingService(
                _db,
                _matchingService,
                _configuration);

            TripiChatMatchingResult matchingResult =
                await matchingService.TimTourAsync(
                    request,
                    cancellationToken);

            // =================================================
            // 9. LOG CAC TIEU CHI THUC TE
            // =================================================

            // Khong ghi nguyen tin nhan khach vao log.
            // Chi ghi cac tieu chi can de debug.

            _logger.LogInformation(
                "[Tripi Matching] " +
                "Destination={Destination}; " +
                "InterestCount={InterestCount}; " +
                "Budget={Budget}; " +
                "BudgetScope={BudgetScope}; " +
                "Days={Days}; " +
                "TourType={TourType}; " +
                "Guests={Guests}; " +
                "DepartureDate={DepartureDate}; " +
                "SearchPerformed={SearchPerformed}; " +
                "TourCount={TourCount}",
                matchingResult.DiemDen,
                matchingResult.SoThich.Count,
                matchingResult.NganSach,
                matchingResult.LoaiNganSach,
                matchingResult.SoNgay,
                matchingResult.LoaiTour,
                matchingResult.SoKhach,
                matchingResult.NgayDuKien,
                matchingResult.DaTimKiem,
                matchingResult.Tours.Count);

            // =================================================
            // 10. TAO CAU TRA LOI DONG BO VOI KET QUA
            // =================================================

            string reply;

            if (matchingResult.DaTimKiem)
            {
                int total = matchingResult.Tours.Count;

                if (total == 0)
                {
                    reply =
                        !string.IsNullOrWhiteSpace(
                            matchingResult.ThongBao)
                            ? matchingResult.ThongBao
                            : "Mình đã tìm theo các điều kiện " +
                              "bạn chia sẻ, nhưng hiện chưa " +
                              "có tour nào phù hợp.";
                }
                else
                {
                    var strongerMatches = matchingResult.Tours
                        .Where(t => t.MatchingScore >= 50)
                        .OrderByDescending(
                            t => t.MatchingScore)
                        .ToList();

                    if (strongerMatches.Count > 0)
                    {
                        var highlighted = strongerMatches
                            .Take(2)
                            .ToList();

                        string names = string.Join(
                            " và ",
                            highlighted.Select(
                                t => $"**{t.TenTour}**"));

                        reply =
                            "Mình vừa xem qua các tour " +
                            "HappyTrip theo nhu cầu của " +
                            "bạn rồi! 🌿 " +
                            $"Trong {total} lựa chọn " +
                            $"được trả về, {names} " +
                            "có điểm phù hợp nổi bật hơn " +
                            "theo các tiêu chí hiện tại.";

                        if (total > strongerMatches.Count)
                        {
                            reply +=
                                "\n\nMột số tour khác vẫn " +
                                "được hiển thị để bạn tham khảo, " +
                                "nhưng mức độ khớp nhu cầu thấp hơn.";
                        }

                        reply +=
                            "\n\nBạn muốn tìm hiểu kỹ hơn " +
                            "về hành trình nào trước?";
                    }
                    else
                    {
                        reply =
                            $"Mình tìm thấy {total} hành trình để bạn tham khảo. " +
                            "Tuy nhiên, các lựa chọn này mới đáp ứng " +
                            "một phần mong muốn của bạn.\n\n" +
                            "Mình đã hiển thị thông tin bên cạnh " +
                            "để bạn xem xét. Nếu muốn, " +
                            "bạn có thể điều chỉnh điều kiện chuyến đi " +
                            "để mình tìm lựa chọn sát nhu cầu hơn nhé.";
                    }
                }
            }
            else
            {
                reply =
                    !string.IsNullOrWhiteSpace(
                        matchingResult.ThongBao)
                        ? matchingResult.ThongBao
                        : chatResult.Reply;
            }

            int? activeTourId = matchingResult.Tours
                .FirstOrDefault()?.IdTour;

            // =================================================
            // 11. RESPONSE
            // =================================================

            return Ok(new
            {
                reply,

                intent = chatResult.Intent,

                canSearchTours =
                    matchingResult.DaTimKiem,

                preferences = new
                {
                    diemDen = matchingResult.DiemDen,

                    soThich = matchingResult.SoThich,

                    nganSach = matchingResult.NganSach,

                    loaiNganSach =
                        matchingResult.LoaiNganSach,

                    soNgay = matchingResult.SoNgay,

                    loaiTour = matchingResult.LoaiTour,

                    soKhach = matchingResult.SoKhach,

                    ngayDuKien =
                        matchingResult.NgayDuKien?
                            .ToString("yyyy-MM-dd")
                },

                searchPerformed =
                    matchingResult.DaTimKiem,

                tours = matchingResult.Tours.Select(t => new
                {
                    idTour = t.IdTour,
                    tenTour = t.TenTour,
                    diaDiem = t.DiaDiem,

                    hinhAnh =
                        TourImagePathHelper.ToTourImageSrc(
                            t.HinhAnh),

                    thoiGian = t.ThoiGian,
                    giaBan = t.GiaBan,
                    soNgay = t.SoNgay,

                    matchingScore = t.MatchingScore,

                    diemSoThich = t.DiemSoThich,
                    diemDiemDen = t.DiemDiemDen,
                    diemNganSach = t.DiemNganSach,

                    lyDoGoiY = t.LyDoGoiY,

                    tenLoaiTour = t.TenLoaiTour,

                    ngayKhoiHanhPhuHop =
                        t.NgayKhoiHanhPhuHop?
                            .ToString("yyyy-MM-dd"),

                    soChoConLai = t.SoChoConLai
                }).ToList(),

                searchMessage =
                    matchingResult.ThongBao,

                activeTourId,

                ragHasEvidence = false,

                ragSources = Array.Empty<object>()
            });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is HttpRequestException ||
            ex is TaskCanceledException ||
            ex is JsonException)
        {
            _logger.LogWarning(
                ex,
                "[Tripi Matching] Integration failed.");

            return Ok(new
            {
                reply =
                    "Mình đã hiểu yêu cầu của bạn, " +
                    "nhưng hiện chưa truy xuất được " +
                    "danh sách tour HappyTrip. " +
                    "Bạn có thể thử tìm lại hoặc " +
                    "tiếp tục trò chuyện cùng mình nhé.",

                intent = chatResult.Intent,

                canSearchTours = false,

                preferences = (object?)null,

                searchPerformed = false,

                tours = Array.Empty<object>(),

                searchMessage =
                    "Chưa thể tìm tour lúc này.",

                activeTourId = (int?)null,

                ragHasEvidence = false,

                ragSources = Array.Empty<object>()
            });
        }
    }

    // ========================================================
    // 12. LOC LICH KHOI HANH THEO THANG
    // ========================================================

    private static bool TryReadMonthQuery(
        string? message,
        out int month,
        out int year)
    {
        month = 0;
        year = 0;

        if (string.IsNullOrWhiteSpace(message) ||
            (!message.Contains(
                "tour",
                StringComparison.OrdinalIgnoreCase) &&
             !message.Contains(
                "khởi hành",
                StringComparison.OrdinalIgnoreCase) &&
             !message.Contains(
                "lịch đi",
                StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        Match match = Regex.Match(
            message,
            @"\btháng\s*(?<month>1[0-2]|0?[1-9])(?:\s*(?:năm|/|-)\s*(?<year>20\d{2}))?\b",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        if (!match.Success ||
            !int.TryParse(
                match.Groups["month"].Value,
                out month))
        {
            return false;
        }

        DateTime now = DateTime.UtcNow.AddHours(7);
        year = now.Year;

        if (match.Groups["year"].Success)
        {
            if (!int.TryParse(
                match.Groups["year"].Value,
                out year))
            {
                return false;
            }
        }
        else if (month < now.Month)
        {
            year++;
        }

        return year is >= 2000 and <= 2100;
    }

        private async Task<IActionResult> LietKeLichTheoThangAsync(
        int thang,
        int nam,
        string message,
        TripiTravelPreferences? savedPreferences,
        CancellationToken cancellationToken,
        bool useStructuredPreferences = false)
    {
        try
        {
            int soKhach;

            string? diemDen;

            decimal? nganSachToiDa;

            bool gioiHanNghiemNgat = false;

            bool nganSachCaNhom;

            if (useStructuredPreferences)
            {
                soKhach = savedPreferences?.SoKhach ?? 0;

                diemDen = savedPreferences?.DiemDen;

                nganSachToiDa = savedPreferences?.NganSach;

                nganSachCaNhom =
                    savedPreferences?.LoaiNganSach == "CaNhom";
            }
            else
            {
                soKhach = TryReadPartySize(message);

                if (soKhach == 0)
                {
                    soKhach = savedPreferences?.SoKhach ?? 0;
                }

                diemDen =
                    TryReadDestination(message) ??
                    savedPreferences?.DiemDen;

                nganSachToiDa =
                    TryReadBudget(
                        message,
                        out gioiHanNghiemNgat);

                nganSachToiDa ??=
                    savedPreferences?.NganSach;

                nganSachCaNhom =
                    message.Contains(
                        "cả nhóm",
                        StringComparison.OrdinalIgnoreCase) ||
                    message.Contains(
                        "tổng ngân sách",
                        StringComparison.OrdinalIgnoreCase) ||
                    (savedPreferences?.LoaiNganSach == "CaNhom" &&
                    !message.Contains(
                        "mỗi người",
                        StringComparison.OrdinalIgnoreCase));
            }

            if (nganSachCaNhom &&
                nganSachToiDa.HasValue &&
                soKhach > 0)
            {
                nganSachToiDa /= soKhach;
            }
            DateOnly ngayBatDau =
                new(nam, thang, 1);

            DateOnly ngayKetThuc =
                ngayBatDau.AddMonths(1);

            DateOnly homNay =
                DateOnly.FromDateTime(
                    DateTime.UtcNow.AddHours(7));

            var lich = await (
                from l in _db.LichKhoiHanhs.AsNoTracking()
                join t in _db.Tours.AsNoTracking()
                    on l.IdTour equals t.IdTour
                join lt in _db.LoaiTours.AsNoTracking()
                    on t.IdLoaiTour equals lt.IdLoaiTour into loai
                from lt in loai.DefaultIfEmpty()
                where t.TrangThai
                    && l.NgayKhoiHanh >= ngayBatDau
                    && l.NgayKhoiHanh < ngayKetThuc
                    && l.NgayKhoiHanh >= homNay
                    && l.SoChoToiDa > 0
                    && l.SoChoConLai > 0
                    && l.SoChoConLai <= l.SoChoToiDa
                    && (l.TrangThai == "Đang mở" ||
                        l.TrangThai == "Còn chỗ" ||
                        l.TrangThai == "Sắp hết chỗ")
                    && (t.GiaKhuyenMai > 0 ||
                        t.GiaGoc > 0)
                orderby l.NgayKhoiHanh, t.IdTour
                select new
                {
                    t.IdTour,
                    t.TenTour,
                    t.DiaDiem,
                    t.HinhAnh,
                    t.ThoiGian,
                    GiaBan = t.GiaKhuyenMai > 0
                        ? t.GiaKhuyenMai
                        : t.GiaGoc,
                    TenLoai = lt == null
                        ? ""
                        : lt.TenLoai,
                    l.NgayKhoiHanh,
                    l.SoChoConLai
                }).ToListAsync(cancellationToken);

            var lichDaLoc = lich.Where(x =>
            {
                decimal giaBan = x.GiaBan ?? 0m;

                bool dungNganSach =
                    !nganSachToiDa.HasValue ||
                    (gioiHanNghiemNgat
                        ? giaBan < nganSachToiDa
                        : giaBan <= nganSachToiDa);

                bool duCho =
                    soKhach == 0 ||
                    x.SoChoConLai >= soKhach;

                bool dungDiemDen =
                    string.IsNullOrWhiteSpace(diemDen) ||
                    x.TenTour.Contains(
                        diemDen,
                        StringComparison.OrdinalIgnoreCase) ||
                    (x.DiaDiem?.Contains(
                        diemDen,
                        StringComparison.OrdinalIgnoreCase) ?? false);

                return dungNganSach &&
                    duCho &&
                    dungDiemDen;
            }).ToList();

            var nhomTour = lichDaLoc
                .GroupBy(x => x.IdTour)
                .ToList();

            var tours = nhomTour.Select(g =>
            {
                var dau = g.First();

                return new
                {
                    idTour = dau.IdTour,
                    tenTour = dau.TenTour,
                    diaDiem = dau.DiaDiem ?? "",

                    hinhAnh =
                        TourImagePathHelper.ToTourImageSrc(
                            dau.HinhAnh ?? ""),

                    thoiGian = dau.ThoiGian ?? "",
                    giaBan = dau.GiaBan ?? 0m,
                    tenLoaiTour = dau.TenLoai,
                    matchingScore = (int?)null,
                    lyDoGoiY = Array.Empty<string>(),

                    ngayKhoiHanhPhuHop =
                        dau.NgayKhoiHanh.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture),

                    soChoConLai =
                        (int?)dau.SoChoConLai
                };
            }).ToList();

            string reply;

            if (tours.Count == 0)
            {
                reply =
                    $"Mình chưa tìm thấy tour đang mở bán và còn chỗ đáp ứng các điều kiện bạn nêu trong tháng {thang:00}/{nam} trên HappyTrip. " +
                    "Bạn có thể đổi một điều kiện hoặc mình sẽ giúp tìm lựa chọn gần nhất nhé.";
            }
            else
            {
                var dong = nhomTour.Select(g =>
                    $"• {g.First().TenTour}: " +
                    string.Join(", ", g.Select(x =>
                        $"{x.NgayKhoiHanh:dd/MM/yyyy} (còn {x.SoChoConLai} chỗ)")));

                reply =
                    $"Trong tháng {thang:00}/{nam}, HappyTrip có {tours.Count} tour với {lichDaLoc.Count} lịch khởi hành đáp ứng các điều kiện bạn nêu:\n\n" +
                    string.Join("\n", dong) +
                    "\n\nMình đã hiển thị các tour bên cạnh. Bạn có thể chọn tour để xem chi tiết nhé.";
            }

            return Ok(new
            {
                reply,
                intent = "list_tours",
                canSearchTours = false,

                preferences = new
                {
                    diemDen,

                    soThich =
                        savedPreferences?.SoThich ??
                        new List<string>(),

                    nganSach =
                        nganSachCaNhom &&
                        nganSachToiDa.HasValue
                            ? nganSachToiDa *
                                (soKhach > 0 ? soKhach : 1)
                            : nganSachToiDa,

                    loaiNganSach =
                        nganSachToiDa.HasValue
                            ? (nganSachCaNhom
                                ? "CaNhom"
                                : "MoiNguoi")
                            : null,

                    soNgay = savedPreferences?.SoNgay,

                    loaiTour = savedPreferences?.LoaiTour,

                    soKhach =
                        soKhach > 0
                            ? (int?)soKhach
                            : null,

                    ngayDuKien = (string?)null,

                    tuNgay =
                        ngayBatDau.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture),

                    denTruocNgay =
                        ngayKetThuc.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture)
                },

                searchPerformed = true,
                tours,

                searchMessage =
                    $"Lịch khởi hành tháng {thang:00}/{nam}",

                activeTourId = (int?)null,
                ragHasEvidence = false,
                ragSources = Array.Empty<object>()
            });
        }
        catch (Exception ex)
            when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "[Tripi] Lỗi truy vấn lịch theo tháng {Month}/{Year}",
                thang,
                nam);

            return StatusCode(503, new
            {
                message =
                    "Tripi chưa tra cứu được lịch khởi hành. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }
    }

    // ========================================================
    // 13. DOC SO KHACH
    // ========================================================

    private static int TryReadPartySize(string message)
    {
        Match match = Regex.Match(
            message,
            @"\b(?<count>\d{1,3})\s*(?:người|khách)\b",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        return match.Success &&
            int.TryParse(
                match.Groups["count"].Value,
                out int count) &&
            count is >= 1 and <= 1000
                ? count
                : 0;
    }

    // ========================================================
    // 14. DOC DIEM DEN
    // ========================================================

    private static string? TryReadDestination(string message)
    {
        Match match = Regex.Match(
            message,
            @"\b(?:đi|đến|tới)\s+(?<place>[\p{L}\s.-]+?)(?=\s+(?:trong|vào|tháng|dưới|với|cho|ngân\s+sách|nhóm|tầm|khoảng)\b|[,?.!]|$)",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        string place = match.Success
            ? match.Groups["place"].Value.Trim()
            : "";

        return place.Length is >= 2 and <= 70
            ? place
            : null;
    }

    // ========================================================
    // 15. DOC NGAN SACH
    // ========================================================

    private static decimal? TryReadBudget(
        string message,
        out bool strict)
    {
        strict = false;

        Match match = Regex.Match(
            message,
            @"(?<operator>dưới|không quá|tối đa|khoảng|tầm|lên|thành|đến mức)\s*(?<amount>\d+(?:[.,]\d+)?)\s*(?<unit>triệu|tr|nghìn|ngàn|k|đồng|đ)\b",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        if (!match.Success ||
            !decimal.TryParse(
                match.Groups["amount"].Value.Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal amount))
        {
            return null;
        }

        strict =
            match.Groups["operator"].Value.Equals(
                "dưới",
                StringComparison.OrdinalIgnoreCase);

        string unit =
            match.Groups["unit"].Value.ToLowerInvariant();

        decimal multiplier =
            unit is "triệu" or "tr"
                ? 1000000m
                : unit is "nghìn" or "ngàn" or "k"
                    ? 1000m
                    : 1m;

        return amount > 0
            ? amount * multiplier
            : null;
    }

    // ========================================================
    // 16. NHAN DIEN DANH SACH TOUR
    // ========================================================

    private static bool LaYeuCauXemDanhSachTour(
        string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        string text = message.Trim().ToLowerInvariant();

        if (text.Contains("không cần xem") ||
            text.Contains("đừng liệt kê") ||
            text.Contains("không muốn xem"))
        {
            return false;
        }

        string[] phrases =
        {
            "có những tour nào",
            "có những tour gì",
            "có những tour du lịch nào",
            "có những tour du lịch gì",
            "hiện có những tour nào",
            "hiện có những tour du lịch nào",
            "đang có những tour nào",
            "đang có những tour du lịch nào",
            "các tour đang có",
            "danh sách tour hiện có",
            "danh sách các tour",
            "danh sách tour du lịch",
            "toàn bộ tour",
            "tất cả các tour",
            "tất cả tour",
            "liệt kê tour hiện có",
            "liệt kê các tour du lịch"
        };

        if (phrases.Any(phrase =>
            text.Contains(
                phrase,
                StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string normalized = text.TrimEnd(
            '?', '!', '.', ' ');

        return normalized is
            "xem danh sách tour" or
            "cho xem danh sách tour" or
            "liệt kê tour" or
            "liệt kê các tour" or
            "danh sách tour";
    }

    // ========================================================
    // 17. DANH MUC TOUR
    // ========================================================

    private async Task<IActionResult> LietKeTourHienCoAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var tours = new List<object>();

            DbConnection connection =
                _db.Database.GetDbConnection();

            bool openedHere =
                connection.State !=
                System.Data.ConnectionState.Open;

            if (openedHere)
            {
                await connection.OpenAsync(
                    cancellationToken);
            }

            try
            {
                await using DbCommand command =
                    connection.CreateCommand();

                command.CommandText = """
                    SELECT TOP (8)
                        t.IdTour,
                        t.TenTour,
                        t.DiaDiem,
                        t.HinhAnh,
                        t.ThoiGian,
                        COALESCE(
                            NULLIF(t.GiaKhuyenMai, 0),
                            NULLIF(t.GiaGoc, 0)
                        ) AS GiaBan,
                        lt.TenLoai
                    FROM dbo.Tour AS t
                    LEFT JOIN dbo.LoaiTour AS lt
                        ON lt.IdLoaiTour = t.IdLoaiTour
                    WHERE t.TrangThai = 1
                    AND COALESCE(
                        NULLIF(t.GiaKhuyenMai, 0),
                        NULLIF(t.GiaGoc, 0)
                    ) > 0
                    ORDER BY t.IdTour DESC;
                    """;

                await using DbDataReader reader =
                    await command.ExecuteReaderAsync(
                        cancellationToken);

                while (await reader.ReadAsync(
                    cancellationToken))
                {
                    int idTour = reader.GetInt32(0);

                    string readText(int index) =>
                        reader.IsDBNull(index)
                            ? ""
                            : Convert.ToString(
                                reader.GetValue(index)) ?? "";

                    tours.Add(new
                    {
                        idTour,
                        tenTour = readText(1),
                        diaDiem = readText(2),

                        hinhAnh =
                            TourImagePathHelper.ToTourImageSrc(
                                readText(3)),

                        thoiGian = readText(4),

                        giaBan = Convert.ToDecimal(
                            reader.GetValue(5)),

                        tenLoaiTour = readText(6),

                        matchingScore = (int?)null,
                        lyDoGoiY = Array.Empty<string>(),
                        ngayKhoiHanhPhuHop = (string?)null,
                        soChoConLai = (int?)null
                    });
                }
            }
            finally
            {
                if (openedHere)
                {
                    await connection.CloseAsync();
                }
            }

            string reply = tours.Count > 0
                ? $"Mình tìm thấy {tours.Count} tour để bạn khám phá trên HappyTrip! 🌿\n\n" +
                "Mình đã hiển thị các tour bên cạnh, " +
                "kèm tên hành trình, thời lượng và giá tham khảo.\n\n" +
                "Đây là danh sách tối đa 8 tour đang hoạt động, " +
                "chưa phải xác nhận lịch khởi hành hoặc số chỗ. " +
                "Bạn có thể chọn một tour để tìm hiểu thêm nhé."
                : "Hiện mình chưa tìm thấy tour đang hoạt động " +
                "có giá bán hợp lệ trong danh mục HappyTrip.";

            return Ok(new
            {
                reply,
                intent = "list_tours",
                canSearchTours = false,

                preferences = (object?)null,

                searchPerformed = true,
                tours,
                searchMessage = "Danh mục tour HappyTrip",

                activeTourId = (int?)null,
                ragHasEvidence = false,
                ragSources = Array.Empty<object>()
            });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Tripi] Khong truy xuat duoc danh muc tour.");

            return StatusCode(503, new
            {
                message =
                    "Tripi chưa lấy được danh sách tour lúc này. " +
                    "Bạn vui lòng thử lại nhé."
            });
        }
    }

    // ========================================================
    // 18. NHAN DIEN YEU CAU TIM TOUR
    // ========================================================

    private static bool LaYeuCauTimTourRoRang(
        string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        string text = message.Trim().ToLowerInvariant();

        // Khong kich hoat khi khach phu dinh yeu cau.
        if (text.Contains("không tìm tour") ||
            text.Contains("đừng tìm tour") ||
            text.Contains("không cần tìm tour"))
        {
            return false;
        }

        string[] yeuCauTimTour =
        {
            "tìm tour",
            "gợi ý tour",
            "đề xuất tour",
            "tư vấn tour",
            "có tour nào",
            "tour nào phù hợp",
            "xem danh sách tour",
            "lọc tour"
        };

        if (yeuCauTimTour.Any(phrase =>
            text.Contains(
                phrase,
                StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Cach noi tu nhien co nhu cau cu the.
        bool muonDi =
            text.Contains("muốn đi ") ||
            text.Contains("tính đi ") ||
            text.Contains("dự định đi ");

        bool coThongTinChuyenDi =
            Regex.IsMatch(
                text,
                @"\d+\s*(ngày|đêm|người|khách)" +
                @"|\d+(?:[.,]\d+)?\s*(triệu|tr)");

        return muonDi && coThongTinChuyenDi;
    }
}