﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using D.Models;

namespace D.Controllers
{
    [Authorize(Roles = "accountant,manager,admin,seller")]
    public class OrderController : Controller
    {
        
      
        private IdbInterface db;

        public OrderController(IdbInterface dbParam)//dependency injection via constructor
        {
           
            db = dbParam;
        }

        public ActionResult Index()
        {
           
            return View("Table");

        }
        public JsonResult Table(dtParam param)
        {
            var query = db.Orders.AsNoTracking();
            Models.DataTableModel.dtOrder res = new Models.DataTableModel.dtOrder();
            res.data = res.GetData(param, query);
            res.draw = param.Draw;
            res.recordsTotal = query.Count();
            res.recordsFiltered = res.Count(param, query);
            return Json(res);
        }

        #region Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
           
            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.Amount = db.Orders.AsNoTracking().Where(ord => ord.OrderID == id)
                            .GroupJoin(
                            db.Orderings,
                            o => o.OrderID,
                            of => of.OrderID,
                            (o, of) => new { amount = of.Sum(oa => oa.ProductQuantity * oa.Product.Price_with_vat) })
                            .FirstOrDefault().amount.Value.ToString("0.00");

            ViewBag.Ord = order;
                        
            ViewBag.Paylist = db.OrderPayments
                .AsNoTracking()
                .Where(g=> g.OrderID == id);
            ViewBag.OfList = db.Orderings.Include("Product")
                .AsNoTracking()
                .Where(d => d.OrderID == id);

            return View(order);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                db.Orders.Add(order);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = order.OrderID });
                //}
                //return View("Error");
            }
            catch (Exception)
            { return View("Error"); }            
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ICollection<Ordering> listOf, [Bind(Exclude = "Ordering")]Order order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Orderings.RemoveRange(db.Orderings.Where(o => o.OrderID == order.OrderID));
                    db.Entry(order).State = EntityState.Modified;
                    db.Orderings.AddRange(listOf);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = order.OrderID });
                }
                return View("Error");
            }
            catch (Exception)
            { return View("Error"); }
            
        }
        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
        [Authorize(Roles = "admin")]
      
        [HandleError(ExceptionType = typeof(System.Data.Entity.Infrastructure.DbUpdateException), View = "ErrorOrders")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            db.Orders.Remove(db.Orders.Find(id));
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

        //getting list of all clients------------------------------------
        [ChildActionOnly]
        public ActionResult AllCustomerEnts()
        {
            return PartialView("_AllCustomerEnts", db.CustomerEnts.AsNoTracking().OrderBy(o=>o.ClientPAN));
        }
        
        public ActionResult AllGoods()
        {

            return PartialView("AllGoods", db.Products.AsNoTracking().OrderBy(o=>o.ProductID));
        }
        [ChildActionOnly]
        public ActionResult AllEmployee()
        {
            return PartialView("AllEmployee", db.Employees.AsNoTracking().OrderBy(o=>o.LastName));
        }

        [ChildActionOnly]
        public ActionResult AllCustomerInds()
        {
            return PartialView("_AllCustomerInds", db.CustomerInds.AsNoTracking().OrderBy(o => o.CustomerIndId));
        }
        //deleting money from order
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteP(OrderPayment payment)
        {
            
            db.OrderPayments.Remove(db.OrderPayments.Find(payment.ID));
            db.SaveChanges();
            return RedirectToAction("Details", new { id = payment.OrderID });
        }

        public ActionResult PeriodReport()
        {
            return View();
        }
        public JsonResult ReportData(dtParam param)
        {
            var query = db.Orders.AsNoTracking().Where(s => s.OrderDate <= param.repEnd && s.OrderDate >= param.repStart);
            Models.DataTableModel.dtOrder res = new Models.DataTableModel.dtOrder();
            res.data = res.GetData(param, query);
            res.draw = param.Draw;
            res.recordsTotal = db.Orders.AsNoTracking().Count();
            res.recordsFiltered = res.Count(param, query);
            return Json(res);
        }

    }
}
