using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Repository.Pattern.Repositories;
using Repository.Pattern.Infrastructure;
using Service.Pattern;
using WebApp.Models;
using WebApp.Repositories;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace WebApp.Services
{
  /// <summary>
  /// File: LogService.cs
  /// Purpose: Within the service layer, you define and implement 
  /// the service interface and the data contracts (or message types).
  /// One of the more important concepts to keep in mind is that a service
  /// should never expose details of the internal processes or 
  /// the business entities used within the application. 
  /// Created Date: 9/19/2019 8:51:51 AM
  /// Author: neo.zhu
  /// Tools: SmartCode MVC5 Scaffolder for Visual Studio 2017
  /// Copyright (c) 2012-2018 All Rights Reserved
  /// </summary>
  public class LogService : Service<Log>, ILogService
  {
    private readonly IRepositoryAsync<Log> repository;
    private readonly IDataTableImportMappingService mappingservice;
    private readonly NLog.ILogger logger;
    public LogService(
      IRepositoryAsync<Log> repository,
      IDataTableImportMappingService mappingservice,
      NLog.ILogger logger
      )
        : base(repository)
    {
      this.repository = repository;
      this.mappingservice = mappingservice;
      this.logger = logger;
    }



    public async Task ImportDataTableAsync(DataTable datatable)
    {
      var mapping = await this.mappingservice.Queryable().Where(x => x.EntitySetName == "Log" && ( ( x.IsEnabled == true ) || ( x.IsEnabled == false && !( x.DefaultValue == null || x.DefaultValue.Equals(string.Empty) ) ) )).ToListAsync();
      if (mapping == null || mapping.Count == 0)
      {
        throw new KeyNotFoundException("没有找到Log对象的Excel导入配置信息，请执行[系统管理/Excel导入配置]");
      }
      foreach (DataRow row in datatable.Rows)
      {
        var item = new Log();
        var requiredfield = mapping.Where(x => x.IsRequired == true).FirstOrDefault()?.SourceFieldName;
        if (requiredfield != null && !row.IsNull(requiredfield) && row[requiredfield] != DBNull.Value && Convert.ToString(row[requiredfield]).Trim() != string.Empty)
        {
          foreach (var field in mapping)
          {
            var defval = field.DefaultValue;
            var contain = datatable.Columns.Contains(field.SourceFieldName ?? "");
            if (contain && !row.IsNull(field.SourceFieldName) && row[field.SourceFieldName] != DBNull.Value && row[field.SourceFieldName].ToString() != string.Empty)
            {
              var logtype = item.GetType();
              var propertyInfo = logtype.GetProperty(field.FieldName);
              var safetype = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
              var safeValue = ( row[field.SourceFieldName] == null ) ? null : Convert.ChangeType(row[field.SourceFieldName], safetype);
              propertyInfo.SetValue(item, safeValue, null);
            }
            else if (!string.IsNullOrEmpty(defval))
            {
              var logtype = item.GetType();
              var propertyInfo = logtype.GetProperty(field.FieldName);
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
               .Where(x => x.EntitySetName == "Log")
               .Select(x => new ExpColumnOpts()
               {
                 EntitySetName = x.EntitySetName,
                 FieldName = x.FieldName,
                 IgnoredColumn = x.IgnoredColumn,
                 SourceFieldName = x.SourceFieldName
               })
               .ToArrayAsync();
      var logs = await this.Query(new LogQuery().Withfilter(filters)).OrderBy(n => n.OrderBy(sort, order)).SelectAsync();
      var datarows =  logs.Select(n => new
      {

        Id = n.Id,
        MachineName = n.MachineName,
        Logged = n.Logged?.ToString("yyyy-MM-dd HH:mm:ss"),
        Level = n.Level,
        Message = n.Message,
        Exception = n.Exception,
        Properties = n.Properties,
        User = n.User,
        Logger = n.Logger,
        Callsite = n.Callsite,
        Resolved = n.Resolved
      }).ToList();
      return await NPOIHelper.ExportExcelAsync("系统日志", datarows, expcolopts);
    }
    public void Delete(int[] id)
    {
      var items = this.Queryable().Where(x => id.Contains(x.Id));
      foreach (var item in items)
      {
        this.Delete(item);
      }

    }
  }
}



