﻿using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System;
using D.Models.DataTableModel;

namespace D.Models
{
    [Authorize(Roles = "seller,manager,admin")]
    public class CustomerEntController : Controller
    {
        private IdbInterface db;//dataContext

        public CustomerEntController(IdbInterface dbParam)//dependency injection via constructor
        {
            db = dbParam;
        
        }
        public ActionResult Index()
        {
            return View("Table");

        }
        //Providing data fof a main table in a json format
        public JsonResult Table(dtParam param)
        {
           
            dtCustomerEnt res = new dtCustomerEnt();
            res.data = res.GetData(param, db.CustomerEnts.AsNoTracking());
            res.draw = param.Draw;
            res.recordsTotal = db.CustomerEnts.AsNoTracking().Count();
            res.recordsFiltered = res.Count(param, db.CustomerEnts.AsNoTracking());
            return Json(res);
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.CustomerEnts.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }



        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConfirmed(CustomerEnt customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.CustomerEnts.Add(customer);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = customer.ClientID });
                }
                return View("Error");
            }
            catch (Exception)
            { return View("Error"); }

           
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( CustomerEnt customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = customer.ClientID });
                }
                return View("Error");
            }
            catch (Exception)
            { return View("Error"); }
        }

        [Authorize(Roles = "admin")]
        [HandleError(ExceptionType = typeof(System.Data.Entity.Infrastructure.DbUpdateException), View = "ErrorClients")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.CustomerEnts.Remove(db.CustomerEnts.Find(id));
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //a report about clients from Mogilev
        public ActionResult ReportMogilev()
        {
            return View("ReportMogilev");
        }
        

        public JsonResult CMogilev(dtParam param)
        {
            IQueryable<CustomerEnt> query = db.CustomerEnts.AsNoTracking().Where(c => c.Address.Contains("могил"));
            dtCustomerEnt res = new dtCustomerEnt();
            res.data = res.GetData(param, query);
            res.draw = param.Draw;
            res.recordsTotal = db.CustomerEnts.AsNoTracking().Count();
            res.recordsFiltered = res.Count(param, query);
            return Json(res);
        }
        //-----------------------------------------------------------------------------

    }
}
