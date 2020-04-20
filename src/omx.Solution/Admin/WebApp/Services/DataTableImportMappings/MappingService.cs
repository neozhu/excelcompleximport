using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Repository.Pattern.Ef6;
using Repository.Pattern.Infrastructure;
using Repository.Pattern.Repositories;
using Service.Pattern;
using WebApp.Models;
using WebApp.Repositories;

namespace WebApp.Services
{
  public class DataTableImportMappingService : Service<DataTableImportMapping>, IDataTableImportMappingService
  {

    private readonly IRepositoryAsync<DataTableImportMapping> _repository;
    public DataTableImportMappingService(IRepositoryAsync<DataTableImportMapping> repository)
        : base(repository) => this._repository = repository;




    public async Task<IEnumerable<EntityInfo>> GetAssemblyEntitiesAsync()
      => await Task.Run(() =>
                        {
                          var list = new List<EntityInfo>();
                          var asm = Assembly.GetExecutingAssembly();
                          list = asm.GetTypes()
                                 .Where(type => typeof(Entity).IsAssignableFrom(type))
                                 .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                                 .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                                 .Select(x => new EntityInfo { EntitySetName = x.DeclaringType.Name, FieldName = x.Name, FieldTypeName = x.PropertyType.Name, IsRequired = x.GetCustomAttributes().Where(f => f.TypeId.ToString().IndexOf("Required") >= 0).Any() })
                                 .OrderBy(x => x.EntitySetName).ThenBy(x => x.FieldName).ToList();

                          return list;
                        });


    public async Task GenerateByEnityNameAsync(string entityName)
    {

      var asm = Assembly.GetExecutingAssembly();
      var list = asm.GetTypes()
             .Where(type => typeof(Entity).IsAssignableFrom(type))
             .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
             .Where(m => m.DeclaringType.Name == entityName &&
                         m.PropertyType.BaseType != typeof(Entity) &&
                       !m.GetCustomAttributes(
                           typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),
                           true).Any()
                   )
             .Select(x =>
                      new EntityInfo
                      {
                        EntitySetName = x.DeclaringType.Name,
                        FieldName = x.Name,
                        FieldTypeName = x.PropertyType.Name,
                        DefaultValue = ( x.GetCustomAttribute(typeof(DefaultValueAttribute)) != null ? ( x.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute ).Value?.ToString() : null ),
                        IsRequired = x.GetCustomAttributes()
                                      .Where(f =>
                                              f.TypeId.ToString().IndexOf("RequiredAttribute") >= 0
                                            ).Any() ||
                                            ( ( x.PropertyType == typeof(int) && !x.GetCustomAttributes().Where(k => k.TypeId.ToString().IndexOf("KeyAttribute") > 0).Any() ) ||
                                             x.PropertyType == typeof(DateTime) ||
                                             x.PropertyType == typeof(decimal)
                                             ),
                        IgnoredColumn = x.GetCustomAttributes().Where(k => k.TypeId.ToString().IndexOf("KeyAttribute") > 0).Any()

                      })
             .OrderBy(x => x.EntitySetName)
             .Where(x => x.FieldTypeName != "ICollection`1").ToList();

      var entityType = asm.GetTypes()
             .Where(type => typeof(Entity).IsAssignableFrom(type)).Where(x => x.Name == entityName).First();

      //this.Queryable().Where(x => x.EntitySetName == entityName).Delete();
      foreach (var item in list)
      {
        var any = await this.Queryable().Where(x => x.EntitySetName == entityName && x.FieldName == item.FieldName).AnyAsync();
        if (!any)
        {
          var row = new DataTableImportMapping
          {
            EntitySetName = item.EntitySetName,
            FieldName = item.FieldName,
            IsRequired = item.IsRequired,
            TypeName = item.FieldTypeName,
            DefaultValue = item.DefaultValue,
            IsEnabled = item.IsRequired,
            IgnoredColumn = item.IgnoredColumn,
            SourceFieldName = AttributeHelper.GetDisplayName(entityType, item.FieldName)
          };
          this.Insert(row);
        }

      }
    }


    public async Task<DataTableImportMapping> FindMappingAsync(string entitySetName, string sourceFieldName) => await this.Queryable().Where(x => x.EntitySetName == entitySetName && x.SourceFieldName == sourceFieldName).FirstOrDefaultAsync();

    public async Task CreateExcelTemplateAsync(string entityname, string filename)
    {
      var mapping = await this.Queryable().Where(x => x.EntitySetName == entityname && x.IsEnabled == true).ToListAsync();
      var finame = new FileInfo(filename);
      if (File.Exists(filename))
      {
        File.Delete(filename);
      }
      using (var wb = new XLWorkbook())
      {
        var ws = wb.Worksheets.Add(entityname);
        var col = 0;
        foreach (var row in mapping)
        {
          col++;
          ws.Cell(1, col).Value = row.SourceFieldName;
          ws.Cell(1, col).Style.Font.Bold = true;
          ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
          ws.Cell(1, col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
          if (row.TypeName == "DateTime")
          {
            //ws.Cell(1, col).DataType = XLDataType.DateTime;
            ws.Cell(1, col).Style.DateFormat.Format = "yyyy-MM-dd";
          }
          else if (row.TypeName.ToLower() == "decimal")
          {
            //ws.Cell(1, col).DataType = XLDataType.Number;
            ws.Cell(1, col).Style.NumberFormat.Format = "#,##0.00";
          }
          else if (row.TypeName.ToLower() == "int")
          {
            //ws.Cell(1, col).DataType = XLDataType.Number;
            ws.Cell(1, col).Style.NumberFormat.Format = "#,##0";
          }
        }
        col = 0;
        foreach (var row in mapping)
        {
          col++;
          ws.Cell(2, col).Value = null;
          //ws.Cell(2, col).Style.Font.Bold = true;
          //ws.Cell(2, col).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
          ws.Cell(2, col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
          if (row.TypeName == "DateTime")
          {
            ws.Cell(2, col).DataType = XLDataType.DateTime;
            ws.Cell(2, col).Style.DateFormat.Format = "yyyy-MM-dd";
          }
          else if (row.TypeName.ToLower() == "decimal")
          {
            ws.Cell(2, col).DataType = XLDataType.Number;
            ws.Cell(2, col).Style.NumberFormat.Format = "#,##0.00";
          }
          else if (row.TypeName.ToLower() == "int")
          {
            ws.Cell(1, col).DataType = XLDataType.Number;
            ws.Cell(2, col).Style.NumberFormat.Format = "#,##0";
          }
        }
        wb.SaveAs(filename);
      }

    }

    public async Task ImportDataTableAsync(DataTable datatable)
    {
      foreach (DataRow row in datatable.Rows)
      {
        var entityName = row["实体名称"].ToString();
        var fieldName = row["字段名"].ToString();
        var any = await this.Queryable().Where(x => x.EntitySetName == entityName && x.FieldName == fieldName).AnyAsync();
        if (!any)
        {
          var item = new DataTableImportMapping()
          {
            EntitySetName = row["实体名称"].ToString(),
            DefaultValue = row["默认值"].ToString(),
            FieldName = row["字段名"].ToString(),
            IgnoredColumn = Convert.ToBoolean(row["是否忽略导出"].ToString()),
            IsEnabled = Convert.ToBoolean(row["是否启用"].ToString()),
            IsRequired = Convert.ToBoolean(row["是否必填"].ToString()),
            SourceFieldName = row["Excel列名"].ToString(),
            RegularExpression = row["验证表达式"].ToString(),
            TypeName = row["类型"].ToString(),
          };
          this.Insert(item);
        }
      }

    }
    public void Delete(int[] id)
    {
      var items = this.Queryable().Where(x => id.Contains(x.Id));
      foreach (var item in items)
      {
        this.Delete(item);
      }

    }
    public async Task<Stream> ExportExcelAsync(string filterRules = "", string sort = "Id", string order = "asc")
    {
      var filters = JsonConvert.DeserializeObject<IEnumerable<filterRule>>(filterRules);
      var query = this.Query(new DataTableImportMappingQuery().Withfilter(filters)).OrderBy(n => n.OrderBy(sort, order)).Select().ToList();
      var datarows = query.Select(n => new
      {
        EntitySetName = n.EntitySetName,
        FieldName = n.FieldName,
        IsRequired = n.IsRequired,
        TypeName = n.TypeName,
        DefaultValue = n.DefaultValue,
        SourceFieldName = n.SourceFieldName,
        IsEnabled = n.IsEnabled,
        IgnoredColumn = n.IgnoredColumn,
        RegularExpression = n.RegularExpression
      }).ToList();
      return await NPOIHelper.ExportExcelAsync("DataTableImportMapping", datarows,null);
    }
  }
}