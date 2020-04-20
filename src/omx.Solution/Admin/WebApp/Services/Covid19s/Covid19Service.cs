using System;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using Repository.Pattern.Repositories;
using Repository.Pattern.Infrastructure;
using Service.Pattern;
using WebApp.Models;
using WebApp.Repositories;
using System.Net.Http;

namespace WebApp.Services
{
  /// <summary>
  /// File: Covid19Service.cs
  /// Purpose: Within the service layer, you define and implement 
  /// the service interface and the data contracts (or message types).
  /// One of the more important concepts to keep in mind is that a service
  /// should never expose details of the internal processes or 
  /// the business entities used within the application. 
  /// Created Date: 2020/4/9 15:06:31
  /// Author: neo.zhu
  /// Tools: SmartCode MVC5 Scaffolder for Visual Studio 2017
  /// Copyright (c) 2012-2018 All Rights Reserved
  /// </summary>
  public class Covid19Service : Service<Covid19>, ICovid19Service
  {
    private readonly IRepositoryAsync<Covid19> repository;
    private readonly IDataTableImportMappingService mappingservice;
    private readonly NLog.ILogger logger;
    public Covid19Service(
      IRepositoryAsync<Covid19> repository,
      IDataTableImportMappingService mappingservice,
      NLog.ILogger logger
      )
        : base(repository)
    {
      this.repository = repository;
      this.mappingservice = mappingservice;
      this.logger = logger;
    }



    public async Task ImportDataTableAsync(DataTable datatable, string username)
    {
      var mapping = await this.mappingservice.Queryable()
                        .Where(x => x.EntitySetName == "Covid19" &&
                           ( x.IsEnabled == true || ( x.IsEnabled == false && x.DefaultValue != null ) )
                           ).ToListAsync();
      if (mapping.Count == 0)
      {
        throw new KeyNotFoundException("没有找到Covid19对象的Excel导入配置信息，请执行[系统管理/Excel导入配置]");
      }
      foreach (DataRow row in datatable.Rows)
      {

        var requiredfield = mapping.Where(x => x.IsRequired == true && x.IsEnabled == true && x.DefaultValue == null).FirstOrDefault()?.SourceFieldName;
        if (requiredfield != null || !row.IsNull(requiredfield))
        {
          var item = new Covid19();
          foreach (var field in mapping)
          {
            var defval = field.DefaultValue;
            var contain = datatable.Columns.Contains(field.SourceFieldName ?? "");
            if (contain && !row.IsNull(field.SourceFieldName))
            {
              var covid19type = item.GetType();
              var propertyInfo = covid19type.GetProperty(field.FieldName);
              var safetype = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
              var safeValue = ( row[field.SourceFieldName] == null ) ? null : Convert.ChangeType(row[field.SourceFieldName], safetype);
              propertyInfo.SetValue(item, safeValue, null);
            }
            else if (!string.IsNullOrEmpty(defval))
            {
              var covid19type = item.GetType();
              var propertyInfo = covid19type.GetProperty(field.FieldName);
              if (string.Equals(defval, "now", StringComparison.OrdinalIgnoreCase) && ( propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(Nullable<DateTime>) ))
              {
                var safetype = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var safeValue = Convert.ChangeType(DateTime.Now, safetype);
                propertyInfo.SetValue(item, safeValue, null);
              }
              else if (string.Equals(defval, "guid", StringComparison.OrdinalIgnoreCase))
              {
                propertyInfo.SetValue(item, Guid.NewGuid().ToString(), null);
              }
              else if (string.Equals(defval, "user", StringComparison.OrdinalIgnoreCase))
              {
                propertyInfo.SetValue(item, username, null);
              }
              else
              {
                var safetype = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var safeValue = Convert.ChangeType(defval, safetype);
                propertyInfo.SetValue(item, safeValue, null);
              }
            }
          }
          this.Insert(item);
        }
      }
    }
    public async Task<Stream> ExportExcelAsync(string filterRules = "", string sort = "Id", string order = "asc")
    {
      var filters = JsonConvert.DeserializeObject<IEnumerable<filterRule>>(filterRules);
      var expcolopts = await this.mappingservice.Queryable()
             .Where(x => x.EntitySetName == "Covid19")
             .Select(x => new ExpColumnOpts()
             {
               EntitySetName = x.EntitySetName,
               FieldName = x.FieldName,
               IgnoredColumn = x.IgnoredColumn,
               SourceFieldName = x.SourceFieldName
             }).ToArrayAsync();

      var covid19s = this.Query(new Covid19Query().Withfilter(filters)).OrderBy(n => n.OrderBy(sort, order)).Select().ToList();
      var datarows = covid19s.Select(n => new
      {

        Id = n.Id,
        country = n.country,
        date = n.date.ToString("yyyy-MM-dd HH:mm:ss"),
        confirmed = n.confirmed,
        deaths = n.deaths,
        recovered = n.recovered
      }).ToList();
      return await NPOIHelper.ExportExcelAsync("新冠状病毒数据", datarows, expcolopts);
    }
    public async Task Delete(int[] id)
    {
      var items = await this.Queryable().Where(x => id.Contains(x.Id)).ToListAsync();
      foreach (var item in items)
      {
        this.Delete(item);
      }

    }
    public async Task SyncData() {
      var url =await this.getPath();
      if (string.IsNullOrEmpty(url))
      {
        return  ;
       }
      using (var httpClient = new HttpClient())
      {
        httpClient.Timeout = TimeSpan.FromMinutes(60);
        var response = await httpClient.GetAsync(url);
        var items = await response.Content.ReadAsAsync<dynamic>();
        foreach (var item in items)
        {
          var country= (string)item["Country/Region"];
          var province = (string)item["Province/State"];
          var dt = DateTime.Parse((string)item.Date);
          var latitude = (decimal)item["Lat"];
          var longitude=(decimal)item["Long"];
          
          var confirmed = (int)( (string)item.Confirmed is null ? 0: item.Confirmed );
            var deaths = (int)( (string)item.Deaths is null ? 0 : item.Deaths );
            var recovered = (int)( (string)item.Recovered is null ? 0 : item.Recovered );
          var any = await this.Queryable().Where(x => x.country == country && x.date == dt && x.province==province).AnyAsync();
            if (!any)
            {
              this.Insert(new Covid19() { country= country,
                 province=province,
                 latitude= latitude,
                  longitude= longitude,
                 confirmed =confirmed,
                  date=dt,
                   deaths=deaths,
                    recovered=recovered
                     });
            }
         
         
        }

        // Now parse with JSON.Net
      }
    }

    private async Task<string> getPath() {
      var url = "https://datahub.io/core/covid-19/datapackage.json";
      using (var httpClient = new HttpClient())
      {
        httpClient.Timeout = TimeSpan.FromMinutes(60);
        var response = await httpClient.GetAsync(url);
        var result = await response.Content.ReadAsAsync<dynamic>();
        var resources = result["resources"];
        foreach (var item in resources)
        {
          if ((string)item["name"] == "time-series-19-covid-combined_json")
          {
            return (string)item["path"];
          }
        }
      }
      return await  Task.FromResult("");
      }
  }
}



