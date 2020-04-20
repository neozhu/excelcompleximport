using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WebApp.App_Helpers.third_party.api
{
  public class covid19api
  {
    public async static Task DownloadData()
    {
      NLog.LogManager.GetCurrentClassLogger().Info("Download covid19 Data begin");
      var db = SqlSugarFactory.CreateSqlSugarClient();
      var url =await getPath();
      if (string.IsNullOrEmpty(url))
      {
        return;
      }
      var sql = "truncate table dbo.covid19";
      await db.Ado.ExecuteCommandAsync(sql);
      using (var httpClient = new HttpClient())
      {
        httpClient.Timeout = TimeSpan.FromMinutes(60);
        var response = await httpClient.GetAsync(url);
        var items = await response.Content.ReadAsAsync<dynamic>();
        foreach (var item in items)
        {
          var country = (string)item["Country/Region"];
          var province = (string)item["Province/State"];
          var dt = DateTime.Parse((string)item.Date);
          var latitude = (decimal)item["Lat"];
          var longitude = (decimal)item["Long"];
          var confirmed = (int)( (string)item.Confirmed is null ? 0 : item.Confirmed );
          var deaths = (int)( (string)item.Deaths is null ? 0 : item.Deaths );
          var recovered = (int)( (string)item.Recovered is null ? 0 : item.Recovered );
          await db.Ado.UseStoredProcedure().ExecuteCommandAsync("[dbo].[SP_InsertCovids19]", new
          {
            date = dt,
            country = country,
            province = province,
            latitude = latitude,
            longitude = longitude,
            confirmed = confirmed,
            deaths = deaths,
            recovered = recovered,
          });

        }



        // Now parse with JSON.Net
      }
      NLog.LogManager.GetCurrentClassLogger().Info("Download covid19 Data completed");
    }


    private async static Task<string> getPath()
    {
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
      return await Task.FromResult("");
    }
  }
}