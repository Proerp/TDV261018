using System;
using System.ComponentModel.DataAnnotations;

namespace TotalDTO.Commons
{
    public interface IMoldBaseDTO
    {
        Nullable<int> MoldID { get; set; }
        [Display(Name = "Khuôn")]
        [UIHint("AutoCompletes/MoldBase")]
        [Required(ErrorMessage = "Vui lòng nhập khuôn")]
        string Code { get; set; }
        string Name { get; set; }
        decimal Quantity { get; set; }
    }

    public class MoldBaseDTO : BaseDTO, IMoldBaseDTO
    {
        public Nullable<int> MoldID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
    }

    public class MoldDTO
    {
    }
}
