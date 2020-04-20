using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Repository.Pattern.Ef6;

namespace WebApp.Models
{
  public partial class Covid19:Entity
  {
    [Key]
    public int Id { get; set; }
    [Display(Name ="国家", Description = "国家")]
    [MaxLength(100)]
    [Required]
    public string country { get; set; }
    [Display(Name = "省/州", Description = "省/州")]
    [MaxLength(100)]
    public string province { get; set; }
    [Display(Name = "日期", Description = "日期")]
    public DateTime date { get; set; }
    [Display(Name = "确诊人数", Description = "确诊人数")]
    public int confirmed { get; set; }
    [Display(Name = "死亡人数", Description = "死亡人数")]
    public int deaths { get; set; }
    [Display(Name = "治愈人数", Description = "治愈人数")]
    public int recovered { get; set; }
    [Display(Name = "latitude", Description = "latitude")]
    public decimal latitude { get; set; }
    [Display(Name = "longitude", Description = "longitude")]
    public decimal longitude { get; set; }
  }
}