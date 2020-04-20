using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using LazyCache;
using NLog;
using WebApp.App_Helpers.third_party.api;
using WebApp.Models;

namespace WebApp.Controllers
{
  [Authorize]
  [RoutePrefix("Home")]
  public class HomeController : Controller
  {
    private readonly IAppCache cache;
    private readonly IMapper mapper;
    private readonly NLog.ILogger logger;
    private readonly SqlSugar.ISqlSugarClient db;

    public HomeController(
      NLog.ILogger logger,
      SqlSugar.ISqlSugarClient db,
      IAppCache cache, IMapper mapper) {
      this.db = db;
      this.cache = cache;
      this.mapper = mapper;
      this.logger = logger;
    }

    public async Task<ActionResult> Index()
    {

      //await covid19api.DownloadData();
      var sql = @"select count(1) from (select country from [dbo].[Covid19]
where confirmed > 0 and CONVERT(date, [date])= CONVERT(date, (select max([date]) from [dbo].[Covid19]))
group by country
) t";
      var sql1 = @"select sum(confirmed) from [dbo].[Covid19]
where confirmed > 0 and CONVERT(date,[date])= CONVERT(date, (select max([date]) from [dbo].[Covid19]))";
      var sql2 = @"select sum(deaths) from [dbo].[Covid19]
where deaths > 0 and CONVERT(date,[date])= CONVERT(date, (select max([date]) from [dbo].[Covid19]))";
      var sql3 = @"select sum(recovered) from [dbo].[Covid19]
where recovered > 0 and CONVERT(date,[date])= CONVERT(date, (select max([date]) from [dbo].[Covid19]))";
      ViewBag.P1 = await this.db.Ado.GetIntAsync(sql);
      ViewBag.P2 = await this.db.Ado.GetIntAsync(sql1);
      ViewBag.P3 = await this.db.Ado.GetIntAsync(sql2);
      ViewBag.P4 = await this.db.Ado.GetIntAsync(sql3);
      return this.View();
    }

    public ActionResult About()
    {
      this.ViewBag.Message = "Your application description page.";

      return this.View();
    }

    public ActionResult GetTime() =>
        //ViewBag.Message = "Your application description page.";

        this.View();
    public ActionResult BlankPage() => this.View();
    public ActionResult AgileBoard() => this.View();


    public ActionResult Contact()
    {
      this.ViewBag.Message = "Your contact page.";

      return this.View();
    }
    public ActionResult Chat() => this.View();




  }
}