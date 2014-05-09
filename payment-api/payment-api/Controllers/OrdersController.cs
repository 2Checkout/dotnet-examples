using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TwoCheckout;

namespace payment_api.Controllers
{
    public class OrdersController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Process()
        {
            TwoCheckoutConfig.SellerID = "sandbox-seller-id";
            TwoCheckoutConfig.PrivateKey = "sandbox-private-key";
            TwoCheckoutConfig.Sandbox = true;

            try
            {
                var Billing = new AuthBillingAddress();
                Billing.addrLine1 = "123 test st";
                Billing.city = "Columbus";
                Billing.zipCode = "43123";
                Billing.state = "OH";
                Billing.country = "USA";
                Billing.name = "Testing Tester";
                Billing.email = "example@2co.com";
                Billing.phone = "5555555555";

                var Customer = new ChargeAuthorizeServiceOptions();
                Customer.total = (decimal)1.00;
                Customer.currency = "USD";
                Customer.merchantOrderId = "123";
                Customer.billingAddr = Billing;
                Customer.token = Request["token"];

                var Charge = new ChargeService();

                var result = Charge.Authorize(Customer);
                ViewBag.Message = result.responseMsg;
            }
            catch (TwoCheckoutException e)
            {
                ViewBag.Message = e.Message.ToString();
            }

            return View();
        }

    }
}
