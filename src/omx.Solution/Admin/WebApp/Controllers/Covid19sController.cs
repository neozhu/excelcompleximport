using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Pattern.UnitOfWork;
using Repository.Pattern.Infrastructure;
using Z.EntityFramework.Plus;
using TrackableEntities;
using WebApp.Models;
using WebApp.Services;
using WebApp.Repositories;
namespace WebApp.Controllers
{
/// <summary>
/// File: Covid19sController.cs
/// Purpose:业务中心/新冠状病毒数据
/// Created Date: 2020/4/9 15:06:32
/// Author: neo.zhu
/// Tools: SmartCode MVC5 Scaffolder for Visual Studio 2017
/// TODO: Registers the type mappings with the Unity container(Mvc.UnityConfig.cs)
/// <![CDATA[
///    container.RegisterType<IRepositoryAsync<Covid19>, Repository<Covid19>>();
///    container.RegisterType<ICovid19Service, Covid19Service>();
/// ]]>
/// Copyright (c) 2012-2018 All Rights Reserved
/// </summary>
    //[Authorize]
    [RoutePrefix("Covid19s")]
	public class Covid19sController : Controller
	{
		private readonly ICovid19Service  covid19Service;
		private readonly IUnitOfWorkAsync unitOfWork;
        private readonly NLog.ILogger logger;
    private readonly SqlSugar.ISqlSugarClient db;
		public Covid19sController (
          ICovid19Service  covid19Service, 
          IUnitOfWorkAsync unitOfWork,
          SqlSugar.ISqlSugarClient db,
          NLog.ILogger logger
          )
		{
			this.covid19Service  = covid19Service;
			this.unitOfWork = unitOfWork;
            this.logger = logger;
      this.db = db;
		}
        		//GET: Covid19s/Index
        //[OutputCache(Duration = 60, VaryByParam = "none")]
        [Route("Index", Name = "新冠状病毒数据", Order = 1)]
		public ActionResult Index() => this.View();

    public async Task<JsonResult> SyncData() {

      try
      {
        var sql = "truncate table dbo.covid19";
        await this.db.Ado.ExecuteCommandAsync(sql);
        await this.covid19Service.SyncData();
        await this.unitOfWork.SaveChangesAsync();
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
       
      catch (Exception e)
      {
        return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
      }
    }
    //获取区域范围数据
    public async Task<JsonResult> GetAreaDataWithDate(DateTime dt)
    {
      //var data =await this.covid19Service.Queryable().Where(x => x.date == dt)
      //     .OrderByDescending(x => x.confirmed)
      //     .Take(20).ToListAsync();
      var sql = @"select  [country],[province],[latitude],[longitude], [date],confirmed ,deaths,recovered from [dbo].[Covid19]
                where [date]=@dt and confirmed > 0 ";
      var result = await this.db.Ado.SqlQueryAsync<dynamic>(sql, new { dt });
      var data = result.Select(x => new {
        country = (string)x.country,
        province = (string)x.province,
        latitude=(decimal)x.latitude,
        longitude = (decimal)x.longitude,
        date = ( (DateTime)x.date ).ToString("yyyy-MM-dd"),
        confirmed = (int)x.confirmed,
        deaths = (int)x.deaths,
        recovered = (int)x.recovered
      }).OrderBy(x => x.confirmed).ToArray();

      return Json(data, JsonRequestBehavior.AllowGet);
    }


    public async Task<JsonResult> GetDataWithDate(DateTime dt) {
      //var data =await this.covid19Service.Queryable().Where(x => x.date == dt)
      //     .OrderByDescending(x => x.confirmed)
      //     .Take(20).ToListAsync();
      var sql = @"select top 20 [country], [date],sum(confirmed) confirmed ,sum(deaths) deaths,sum(recovered) recovered from [dbo].[Covid19]
                where [date]=@dt
                group by[country], [date]
                order by sum(confirmed) desc ";
      var result = await this.db.Ado.SqlQueryAsync<dynamic>(sql, new { dt });
      var data = result.Select(x => new {
        country=(string)x.country,
        date = ( (DateTime)x.date ).ToString("yyyy-MM-dd"),
        confirmed = (int)x.confirmed,
        deaths = (int)x.deaths,
        recovered = (int)x.recovered
      }).OrderBy(x => x.confirmed).ToArray();

      return Json(data, JsonRequestBehavior.AllowGet);
    }
    public async Task<JsonResult> GetSumDataWithRange(DateTime dt)
    {
      var sql = @"select [date],sum(confirmed) confirmed ,sum(deaths) deaths,sum(recovered) recovered from [dbo].[Covid19]
                where [date] <= @dt
                group by[date]
                order by[date]";
      var result = await this.db.Ado.SqlQueryAsync<dynamic>(sql, new { dt });
      var data = result.Select(x => new { date = ((DateTime)x.date).ToString("yyyy-MM-dd"),
        confirmed = (int)x.confirmed,
        deaths = (int)x.deaths,
        recovered = (int)x.recovered
      });
       
      return Json(data , JsonRequestBehavior.AllowGet);
    }
    public async Task<JsonResult> GetSumDataWithChinaRange(DateTime dt)
    {
      var sql = @"select [date],sum(confirmed) confirmed ,sum(deaths) deaths,sum(recovered) recovered from [dbo].[Covid19]
                where [date] <= @dt and country='China' 
                group by[date]
                order by[date]";
      var result = await this.db.Ado.SqlQueryAsync<dynamic>(sql, new { dt });
      var data = result.Select(x => new {
        date = ( (DateTime)x.date ).ToString("yyyy-MM-dd"),
        confirmed = (int)x.confirmed,
        deaths = (int)x.deaths,
        recovered = (int)x.recovered
      });

      return Json(data, JsonRequestBehavior.AllowGet);
    }

    public async Task<JsonResult> GetTrainingData()
    {
      var sql = @"select [date],sum(confirmed) confirmed ,sum(deaths) deaths,sum(recovered) recovered from [dbo].[Covid19]
                group by[date]
                order by[date]";
      var result = await this.db.Ado.SqlQueryAsync<dynamic>(sql);
      var data = result.Select(x => new {
        date = ( (DateTime)x.date ).ToString("yyyy-MM-dd"),
        confirmed = (int)x.confirmed,
        deaths = (int)x.deaths,
        recovered = (int)x.recovered
      });

      return Json(data, JsonRequestBehavior.AllowGet);
    }
    //Get :Covid19s/GetData
    //For Index View datagrid datasource url

    [HttpGet]
        //[OutputCache(Duration = 10, VaryByParam = "*")]
		 public async Task<JsonResult> GetData(int page = 1, int rows = 10, string sort = "Id", string order = "asc", string filterRules = "")
		{
			var filters = JsonConvert.DeserializeObject<IEnumerable<filterRule>>(filterRules);
			var pagerows  = (await this.covid19Service
						               .Query(new Covid19Query().Withfilter(filters))
							           .OrderBy(n=>n.OrderBy(sort,order))
							           .SelectPageAsync(page, rows, out var totalCount))
                                       .Select(  n => new { 

    Id = n.Id,
    country = n.country,
    date = n.date.ToString("yyyy-MM-dd HH:mm:ss"),
    confirmed = n.confirmed,
    deaths = n.deaths,
    recovered = n.recovered
}).ToList();
			var pagelist = new { total = totalCount, rows = pagerows };
			return Json(pagelist, JsonRequestBehavior.AllowGet);
		}
        //easyui datagrid post acceptChanges 
		[HttpPost]
		public async Task<JsonResult> SaveData(Covid19[] covid19s)
		{
            if (covid19s == null)
            {
                throw new ArgumentNullException(nameof(covid19s));
            }
            if (ModelState.IsValid)
			{
            try{
               foreach (var item in covid19s)
               {
                 this.covid19Service.ApplyChanges(item);
               }
			   var result = await this.unitOfWork.SaveChangesAsync();
			   return Json(new {success=true,result}, JsonRequestBehavior.AllowGet);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                var errormessage = string.Join(",", e.EntityValidationErrors.Select(x => x.ValidationErrors.FirstOrDefault()?.PropertyName + ":" + x.ValidationErrors.FirstOrDefault()?.ErrorMessage));
                 return Json(new { success = false, err = errormessage }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
                {
                    return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
                }
		    }
            else
            {
                var modelStateErrors = string.Join(",", ModelState.Keys.SelectMany(key => ModelState[key].Errors.Select(n => n.ErrorMessage)));
                return Json(new { success = false, err = modelStateErrors }, JsonRequestBehavior.AllowGet);
            }
        
        }
						//GET: Covid19s/Details/:id
		public ActionResult Details(int id)
		{
			
			var covid19 = this.covid19Service.Find(id);
			if (covid19 == null)
			{
				return HttpNotFound();
			}
			return View(covid19);
		}
        //GET: Covid19s/GetItem/:id
        [HttpGet]
        public async Task<JsonResult> GetItem(int id) {
            var  covid19 = await this.covid19Service.FindAsync(id);
            return Json(covid19,JsonRequestBehavior.AllowGet);
        }
		//GET: Covid19s/Create
        		public ActionResult Create()
				{
			var covid19 = new Covid19();
			//set default value
			return View(covid19);
		}
		//POST: Covid19s/Create
		//To protect from overposting attacks, please enable the specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(Covid19 covid19)
		{
			if (covid19 == null)
            {
                throw new ArgumentNullException(nameof(covid19));
            } 
            if (ModelState.IsValid)
			{
                try{ 
				this.covid19Service.Insert(covid19);
				var result = await this.unitOfWork.SaveChangesAsync();
                return Json(new { success = true,result }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                   var errormessage = string.Join(",", e.EntityValidationErrors.Select(x => x.ValidationErrors.FirstOrDefault()?.PropertyName + ":" + x.ValidationErrors.FirstOrDefault()?.ErrorMessage));
                   return Json(new { success = false, err = errormessage }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
                }
			    //DisplaySuccessMessage("Has update a covid19 record");
			}
			else {
			   var modelStateErrors =string.Join(",", this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors.Select(n=>n.ErrorMessage)));
			   return Json(new { success = false, err = modelStateErrors }, JsonRequestBehavior.AllowGet);
			   //DisplayErrorMessage(modelStateErrors);
			}
			//return View(covid19);
		}

        //新增对象初始化
        [HttpGet]
        public async Task<JsonResult> NewItem() {
            var covid19 = await Task.Run(() => {
                return new Covid19();
                });
            return Json(covid19, JsonRequestBehavior.AllowGet);
        }

         
		//GET: Covid19s/Edit/:id
		public ActionResult Edit(int id)
		{
			var covid19 = this.covid19Service.Find(id);
			if (covid19 == null)
			{
				return HttpNotFound();
			}
			return View(covid19);
		}
		//POST: Covid19s/Edit/:id
		//To protect from overposting attacks, please enable the specific properties you want to bind to, for more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Covid19 covid19)
		{
            if (covid19 == null)
            {
                throw new ArgumentNullException(nameof(covid19));
            }
			if (ModelState.IsValid)
			{
				covid19.TrackingState = TrackingState.Modified;
				                try{
				this.covid19Service.Update(covid19);
				                
				var result = await this.unitOfWork.SaveChangesAsync();
                return Json(new { success = true,result = result }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    var errormessage = string.Join(",", e.EntityValidationErrors.Select(x => x.ValidationErrors.FirstOrDefault()?.PropertyName + ":" + x.ValidationErrors.FirstOrDefault()?.ErrorMessage));
                    return Json(new { success = false, err = errormessage }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
                }
				
				//DisplaySuccessMessage("Has update a Covid19 record");
				//return RedirectToAction("Index");
			}
			else {
			var modelStateErrors =string.Join(",", this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors.Select(n=>n.ErrorMessage)));
			return Json(new { success = false, err = modelStateErrors }, JsonRequestBehavior.AllowGet);
			//DisplayErrorMessage(modelStateErrors);
			}
						//return View(covid19);
		}
        //删除当前记录
		//GET: Covid19s/Delete/:id
        [HttpGet]
		public async Task<ActionResult> Delete(int id)
		{
          try{
               await this.covid19Service.Queryable().Where(x => x.Id == id).DeleteAsync();
               return Json(new { success = true }, JsonRequestBehavior.AllowGet);
           }
           catch (System.Data.Entity.Validation.DbEntityValidationException e)
           {
                var errormessage = string.Join(",", e.EntityValidationErrors.Select(x => x.ValidationErrors.FirstOrDefault()?.PropertyName + ":" + x.ValidationErrors.FirstOrDefault()?.ErrorMessage));
                return Json(new { success = false, err = errormessage }, JsonRequestBehavior.AllowGet);
           }
           catch (Exception e)
           {
                return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
           }
		}
		 
       
 

        //删除选中的记录
        [HttpPost]
        public async Task<JsonResult> DeleteChecked(int[] id) {
           if (id == null)
           {
                throw new ArgumentNullException(nameof(id));
           }
           try{
               await this.covid19Service.Delete(id);
               await this.unitOfWork.SaveChangesAsync();
               return Json(new { success = true }, JsonRequestBehavior.AllowGet);
           }
           catch (System.Data.Entity.Validation.DbEntityValidationException e)
           {
                    var errormessage = string.Join(",", e.EntityValidationErrors.Select(x => x.ValidationErrors.FirstOrDefault()?.PropertyName + ":" + x.ValidationErrors.FirstOrDefault()?.ErrorMessage));
                    return Json(new { success = false, err = errormessage }, JsonRequestBehavior.AllowGet);
           }
           catch (Exception e)
           {
                    return Json(new { success = false, err = e.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
           }
        }
		//导出Excel
		[HttpPost]
		public async Task<ActionResult> ExportExcel( string filterRules = "",string sort = "Id", string order = "asc")
		{
			var fileName = "covid19s_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
			var stream = await this.covid19Service.ExportExcelAsync(filterRules,sort, order );
			return File(stream, "application/vnd.ms-excel", fileName);
		}
		private void DisplaySuccessMessage(string msgText) => TempData["SuccessMessage"] = msgText;
        private void DisplayErrorMessage(string msgText) => TempData["ErrorMessage"] = msgText;
		 
	}
}
