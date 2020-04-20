using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace WebApp.Models
{
// <copyright file="Covid19Metadata.cs" tool="martCode MVC5 Scaffolder">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>neo.zhu</author>
// <date>2020/4/9 15:06:33 </date>
// <summary>Class representing a Metadata entity </summary>
    //[MetadataType(typeof(Covid19Metadata))]
    public partial class Covid19
    {
    }

    public partial class Covid19Metadata
    {
        [Required(ErrorMessage = "Please enter : Id")]
        [Display(Name = "Id",Description ="Id",Prompt = "Id",ResourceType = typeof(resource.Covid19))]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter : 国家")]
        [Display(Name = "country",Description ="国家",Prompt = "国家",ResourceType = typeof(resource.Covid19))]
        [MaxLength(100)]
        public string country { get; set; }

        [Required(ErrorMessage = "Please enter : 日期")]
        [Display(Name = "date",Description ="日期",Prompt = "日期",ResourceType = typeof(resource.Covid19))]
        public DateTime date { get; set; }

        [Required(ErrorMessage = "Please enter : 确诊人数")]
        [Display(Name = "confirmed",Description ="确诊人数",Prompt = "确诊人数",ResourceType = typeof(resource.Covid19))]
        public int confirmed { get; set; }

        [Required(ErrorMessage = "Please enter : 死亡人数")]
        [Display(Name = "deaths",Description ="死亡人数",Prompt = "死亡人数",ResourceType = typeof(resource.Covid19))]
        public int deaths { get; set; }

        [Required(ErrorMessage = "Please enter : 治愈人数")]
        [Display(Name = "recovered",Description ="治愈人数",Prompt = "治愈人数",ResourceType = typeof(resource.Covid19))]
        public int recovered { get; set; }

        [Display(Name = "CreatedDate",Description ="创建时间",Prompt = "创建时间",ResourceType = typeof(resource.Covid19))]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "CreatedBy",Description ="创建用户",Prompt = "创建用户",ResourceType = typeof(resource.Covid19))]
        [MaxLength(20)]
        public string CreatedBy { get; set; }

        [Display(Name = "LastModifiedDate",Description ="最后更新时间",Prompt = "最后更新时间",ResourceType = typeof(resource.Covid19))]
        public DateTime LastModifiedDate { get; set; }

        [Display(Name = "LastModifiedBy",Description ="最后更新用户",Prompt = "最后更新用户",ResourceType = typeof(resource.Covid19))]
        [MaxLength(20)]
        public string LastModifiedBy { get; set; }

        [Required(ErrorMessage = "Please enter : Tenant Id")]
        [Display(Name = "TenantId",Description ="Tenant Id",Prompt = "Tenant Id",ResourceType = typeof(resource.Covid19))]
        public int TenantId { get; set; }

    }

}
