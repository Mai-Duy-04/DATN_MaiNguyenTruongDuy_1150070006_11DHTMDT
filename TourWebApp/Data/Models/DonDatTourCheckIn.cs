using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourWebApp.Data.Models;

[Table("DonDatTourCheckIn")]
public partial class DonDatTourCheckIn
{
    [Key]
    public int IdCheckIn { get; set; }

    public int IdDon { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime NgayCheckIn { get; set; }

    public int SoKhachCheckIn { get; set; }

    public int IdNguoiXacNhan { get; set; }

    [ForeignKey(nameof(IdDon))]
    public virtual DonDatTour IdDonNavigation { get; set; } = null!;
}
