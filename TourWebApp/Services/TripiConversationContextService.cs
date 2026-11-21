using System.Globalization;

namespace TourWebApp.Services;

public sealed class TripiFieldUpdate<T>
{
    public string Action { get; set; } = "keep";

    public T? Value { get; set; }
}

public sealed class TripiContextUpdate
{
    public TripiFieldUpdate<string>? DiemDen { get; set; }

    public TripiFieldUpdate<List<string>>? SoThich { get; set; }

    public TripiFieldUpdate<decimal?>? NganSach { get; set; }

    public TripiFieldUpdate<string>? LoaiNganSach { get; set; }

    public TripiFieldUpdate<int?>? SoNgay { get; set; }

    public TripiFieldUpdate<string>? LoaiTour { get; set; }

    public TripiFieldUpdate<int?>? SoKhach { get; set; }

    public TripiFieldUpdate<string>? NgayDuKien { get; set; }

    public TripiFieldUpdate<string>? TuNgay { get; set; }

    public TripiFieldUpdate<string>? DenTruocNgay { get; set; }
}

public sealed class TripiConversationContextService
{
    public TripiTravelPreferences ApplyUpdate(
        TripiTravelPreferences? current,
        TripiContextUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var result = Clone(current);

        result.DiemDen = ApplyString(
            result.DiemDen,
            update.DiemDen,
            200);

        result.SoThich = ApplyInterests(
            result.SoThich,
            update.SoThich);

        result.NganSach = ApplyDecimal(
            result.NganSach,
            update.NganSach);

        result.LoaiNganSach = ApplyString(
            result.LoaiNganSach,
            update.LoaiNganSach,
            30);

        result.SoNgay = ApplyInteger(
            result.SoNgay,
            update.SoNgay,
            1,
            365);

        result.LoaiTour = ApplyString(
            result.LoaiTour,
            update.LoaiTour,
            200);

        result.SoKhach = ApplyInteger(
            result.SoKhach,
            update.SoKhach,
            1,
            1000);

        result.NgayDuKien = ApplyDate(
            result.NgayDuKien,
            update.NgayDuKien);

        result.TuNgay = ApplyDate(
            result.TuNgay,
            update.TuNgay);

        result.DenTruocNgay = ApplyDate(
            result.DenTruocNgay,
            update.DenTruocNgay);

        if (result.NganSach == null)
        {
            result.LoaiNganSach = null;
        }

        if (result.LoaiNganSach != null &&
            result.LoaiNganSach != "MoiNguoi" &&
            result.LoaiNganSach != "CaNhom")
        {
            throw new ArgumentException(
                "Phạm vi ngân sách không hợp lệ.");
        }

        ValidateDateRange(result);

        return result;
    }

    private static TripiTravelPreferences Clone(
        TripiTravelPreferences? current)
    {
        if (current == null)
        {
            return new TripiTravelPreferences();
        }

        return new TripiTravelPreferences
        {
            DiemDen = current.DiemDen,

            SoThich = current.SoThich?.ToList()
                ?? new List<string>(),

            NganSach = current.NganSach,

            LoaiNganSach = current.LoaiNganSach,

            SoNgay = current.SoNgay,

            LoaiTour = current.LoaiTour,

            SoKhach = current.SoKhach,

            NgayDuKien = current.NgayDuKien,

            TuNgay = current.TuNgay,

            DenTruocNgay = current.DenTruocNgay
        };
    }

    private static string? ApplyString(
        string? current,
        TripiFieldUpdate<string>? update,
        int maxLength)
    {
        if (update == null ||
            IsAction(update.Action, "keep"))
        {
            return current;
        }

        if (IsAction(update.Action, "clear"))
        {
            return null;
        }

        EnsureSetAction(update.Action);

        string? value = update.Value?.Trim();

        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > maxLength)
        {
            throw new ArgumentException(
                "Giá trị điều kiện không hợp lệ.");
        }

        return value;
    }

    private static decimal? ApplyDecimal(
        decimal? current,
        TripiFieldUpdate<decimal?>? update)
    {
        if (update == null ||
            IsAction(update.Action, "keep"))
        {
            return current;
        }

        if (IsAction(update.Action, "clear"))
        {
            return null;
        }

        EnsureSetAction(update.Action);

        if (!update.Value.HasValue ||
            update.Value.Value <= 0 ||
            update.Value.Value > 1000000000m)
        {
            throw new ArgumentException(
                "Ngân sách không hợp lệ.");
        }

        return update.Value.Value;
    }

    private static int? ApplyInteger(
        int? current,
        TripiFieldUpdate<int?>? update,
        int min,
        int max)
    {
        if (update == null ||
            IsAction(update.Action, "keep"))
        {
            return current;
        }

        if (IsAction(update.Action, "clear"))
        {
            return null;
        }

        EnsureSetAction(update.Action);

        if (!update.Value.HasValue ||
            update.Value.Value < min ||
            update.Value.Value > max)
        {
            throw new ArgumentException(
                "Giá trị số không hợp lệ.");
        }

        return update.Value.Value;
    }

    private static List<string> ApplyInterests(
        List<string>? current,
        TripiFieldUpdate<List<string>>? update)
    {
        if (update == null ||
            IsAction(update.Action, "keep"))
        {
            return current?.ToList()
                ?? new List<string>();
        }

        if (IsAction(update.Action, "clear"))
        {
            return new List<string>();
        }

        EnsureSetAction(update.Action);

        if (update.Value == null ||
            update.Value.Count > 20 ||
            update.Value.Any(x =>
                string.IsNullOrWhiteSpace(x) ||
                x.Length > 100))
        {
            throw new ArgumentException(
                "Danh sách sở thích không hợp lệ.");
        }

        return update.Value
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? ApplyDate(
        string? current,
        TripiFieldUpdate<string>? update)
    {
        if (update == null ||
            IsAction(update.Action, "keep"))
        {
            return current;
        }

        if (IsAction(update.Action, "clear"))
        {
            return null;
        }

        EnsureSetAction(update.Action);

        if (!DateOnly.TryParseExact(
            update.Value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly date))
        {
            throw new ArgumentException(
                "Ngày dự kiến không hợp lệ.");
        }

        return date.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);
    }

    private static void ValidateDateRange(
        TripiTravelPreferences preferences)
    {
        bool hasStart =
            !string.IsNullOrWhiteSpace(
                preferences.TuNgay);

        bool hasEnd =
            !string.IsNullOrWhiteSpace(
                preferences.DenTruocNgay);

        if (!hasStart && !hasEnd)
        {
            return;
        }

        if (!hasStart || !hasEnd)
        {
            throw new ArgumentException(
                "Khoảng ngày phải có đủ ngày bắt đầu và kết thúc.");
        }

        if (!DateOnly.TryParseExact(
                preferences.TuNgay,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly start) ||
            !DateOnly.TryParseExact(
                preferences.DenTruocNgay,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly end))
        {
            throw new ArgumentException(
                "Khoảng ngày không hợp lệ.");
        }

        int totalDays = end.DayNumber - start.DayNumber;

        if (totalDays < 1 || totalDays > 366)
        {
            throw new ArgumentException(
                "Khoảng thời gian tìm tour không hợp lệ.");
        }
    }

    private static bool IsAction(
        string? actual,
        string expected)
    {
        return string.Equals(
            actual?.Trim(),
            expected,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureSetAction(
        string? action)
    {
        if (!IsAction(action, "set"))
        {
            throw new ArgumentException(
                "Thao tác cập nhật điều kiện không hợp lệ.");
        }
    }
}