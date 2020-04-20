using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity.SqlServer;
using Repository.Pattern.Repositories;
using Repository.Pattern.Ef6;
using System.Web.WebPages;
using WebApp.Models;

namespace WebApp.Repositories
{
/// <summary>
/// File: LogQuery.cs
/// Purpose: easyui datagrid filter query 
/// Created Date: 9/19/2019 8:51:49 AM
/// Author: neo.zhu
/// Tools: SmartCode MVC5 Scaffolder for Visual Studio 2017
/// Copyright (c) 2012-2018 All Rights Reserved
/// </summary>
   public class LogQuery:QueryObject<Log>
   {
		public LogQuery Withfilter(IEnumerable<filterRule> filters)
        {
      if (filters != null)
      {
        foreach (var rule in filters)
        {
          if (rule.field == "Id" && !string.IsNullOrEmpty(rule.value) && rule.value.IsInt())
          {
            var val = Convert.ToInt32(rule.value);
            switch (rule.op)
            {
              case "equal":
                And(x => x.Id == val);
                break;
              case "notequal":
                And(x => x.Id != val);
                break;
              case "less":
                And(x => x.Id < val);
                break;
              case "lessorequal":
                And(x => x.Id <= val);
                break;
              case "greater":
                And(x => x.Id > val);
                break;
              case "greaterorequal":
                And(x => x.Id >= val);
                break;
              default:
                And(x => x.Id == val);
                break;
            }
          }
          if (rule.field == "MachineName" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.MachineName.Contains(rule.value));
          }
          if (rule.field == "Logged" && !string.IsNullOrEmpty(rule.value))
          {
            if (rule.op == "between")
            {
              var datearray = rule.value.Split(new char[] { '-' });
              var start = Convert.ToDateTime(datearray[0]);
              var end = Convert.ToDateTime(datearray[1]);

              And(x => SqlFunctions.DateDiff("d", start, x.Logged) >= 0);
              And(x => SqlFunctions.DateDiff("d", end, x.Logged) <= 0);
            }
          }
          if (rule.field == "Level" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Level.Contains(rule.value));
          }
          if (rule.field == "Message" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Message.Contains(rule.value));
          }
          if (rule.field == "Exception" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Exception.Contains(rule.value));
          }
          if (rule.field == "Properties" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Properties.Contains(rule.value));
          }
          if (rule.field == "User" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.User.Contains(rule.value));
          }
          if (rule.field == "Logger" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Logger.Contains(rule.value));
          }
          if (rule.field == "Callsite" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.Callsite.Contains(rule.value));
          }
          if (rule.field == "Resolved" && !string.IsNullOrEmpty(rule.value) && rule.value.IsBool())
          {
            var boolval = Convert.ToBoolean(rule.value);
            And(x => x.Resolved == boolval);
          }
          if (rule.field == "CreatedDate" && !string.IsNullOrEmpty(rule.value))
          {
            if (rule.op == "between")
            {
              var datearray = rule.value.Split(new char[] { '-' });
              var start = Convert.ToDateTime(datearray[0]);
              var end = Convert.ToDateTime(datearray[1]);

              And(x => SqlFunctions.DateDiff("d", start, x.CreatedDate) >= 0);
              And(x => SqlFunctions.DateDiff("d", end, x.CreatedDate) <= 0);
            }
          }
          if (rule.field == "CreatedBy" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.CreatedBy.Contains(rule.value));
          }
          if (rule.field == "LastModifiedDate" && !string.IsNullOrEmpty(rule.value))
          {
            if (rule.op == "between")
            {
              var datearray = rule.value.Split(new char[] { '-' });
              var start = Convert.ToDateTime(datearray[0]);
              var end = Convert.ToDateTime(datearray[1]);

              And(x => SqlFunctions.DateDiff("d", start, x.LastModifiedDate) >= 0);
              And(x => SqlFunctions.DateDiff("d", end, x.LastModifiedDate) <= 0);
            }
          }
          if (rule.field == "LastModifiedBy" && !string.IsNullOrEmpty(rule.value))
          {
            And(x => x.LastModifiedBy.Contains(rule.value));
          }
          if (rule.field == "TenantId" && !string.IsNullOrEmpty(rule.value) && rule.value.IsInt())
          {
            var val = Convert.ToInt32(rule.value);
            switch (rule.op)
            {
              case "equal":
                And(x => x.TenantId == val);
                break;
              case "notequal":
                And(x => x.TenantId != val);
                break;
              case "less":
                And(x => x.TenantId < val);
                break;
              case "lessorequal":
                And(x => x.TenantId <= val);
                break;
              case "greater":
                And(x => x.TenantId > val);
                break;
              case "greaterorequal":
                And(x => x.TenantId >= val);
                break;
              default:
                And(x => x.TenantId == val);
                break;
            }
          }

        }
      }
      else
      {
        And(x => x.Resolved == false);
      }
            return this;
        }
    }
}
