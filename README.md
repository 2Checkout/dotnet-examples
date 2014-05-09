2Checkout Payment API .NET Tutorial
=========================

In this tutorial we will walk through integrating the 2Checkout Payment API to securely tokenize and charge a credit card using the [2Checkout .NET library](https://www.2checkout.com/documentation/libraries/net). You will need a 2Checkout sandbox account to complete the tutorial so if you have not already, [signup for an account](https://sandbox.2checkout.com/sandbox/signup) and [generate your Payment API keys](https://www.2checkout.com/documentation/sandbox/payment-api-testing).

----

### Application Setup

For our example application, we will be using MVC 4 targeting .NET 4.5.

To start off, in Visual Studio create a new empty MVC 4 application and name it 'payment-api'. In your projects solution explorer, right click on References and open the NuGet package manager. Here you need to install Json.NET version 6.0.3 as it is a dependency for the 2Checkout .NET library.

We also need to [download the 2Checkout .NET library](https://github.com/2Checkout/2checkout-dotnet) and add the included TwoCheckout.dll as a reference. This provides us with a simple bindings to the API, INS and Checkout process so that we can integrate each feature with only a few lines of code. In this example, we will only be using the Payment API functionality of the library.

Create a new view directory named Orders and then add 2 new views, Index.aspx and Process.aspx.
_(When creating the views, leave "Create as partial" and "Use a layout or master page" unchecked.)_

Now set your default route to Orders/Index under App_Start -> RouteConfig.cs.

```
public static void RegisterRoutes(RouteCollection routes)
{
    routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

    routes.MapRoute(
        name: "Default",
        url: "{controller}/{action}/{id}",
        defaults: new { controller = "Orders", action = "Index", id = UrlParameter.Optional }
    );
}
```

----

# Create a Token

Open the 'Orders/Index' view and create a basic HTML skeleton.

```
<!DOCTYPE html>
<html>
    <head runat="server">
        <meta name="viewport" content="width=device-width" />
        <title>Index</title>
    </head>
    <body>

    </body>
</html>
```

Next add a basic credit card form that allows our buyer to enter in their card number, expiration month and year and CVC. Set the form's action to "/orders/process" so that it will POST to the 'Process' action that we will setup when we create our 'Orders' controller later.

```
<form id="myCCForm" action="/orders/process" method="post">
    <input id="token" name="token" type="hidden" value="">
    <div>
        <label>
            <span>Card Number</span>
        </label>
        <input id="ccNo" type="text" size="20" value="" autocomplete="off" required />
    </div>
    <div>
        <label>
            <span>Expiration Date (MM/YYYY)</span>
        </label>
        <input type="text" size="2" id="expMonth" required />
        <span> / </span>
        <input type="text" size="2" id="expYear" required />
    </div>
    <div>
        <label>
            <span>CVC</span>
        </label>
        <input id="cvv" size="4" type="text" value="" autocomplete="off" required />
    </div>
    <input type="submit" value="Submit Payment">
</form>
```

Notice that we have a no 'name' attributes on the input elements that collect the credit card information. This will ensure that no sensitive card data touches your server when the form is submitted. Also, we include a hidden input element for the token which we will submit to our server to make the authorization request.

Now we can add our JavaScript to make the token request call. Replace 'sandbox-seller-id' and 'sandbox-publishable-key' with your sandbox credentials.

```
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.0/jquery.min.js"></script>
<script src="https://www.2checkout.com/checkout/api/2co.min.js"></script>

<script>
    // Called when token created successfully.
    var successCallback = function(data) {
        var myForm = document.getElementById('myCCForm');

        // Set the token as the value for the token input
        myForm.token.value = data.response.token.token;

        // IMPORTANT: Here we call `submit()` on the form element directly instead of using jQuery to prevent and infinite token request loop.
        myForm.submit();
    };

    // Called when token creation fails.
    var errorCallback = function(data) {
        if (data.errorCode === 200) {
            tokenRequest();
        } else {
            alert(data.errorMsg);
        }
    };

    var tokenRequest = function() {
        // Setup token request arguments
        var args = {
            sellerId: "sandbox-seller-id",
            publishableKey: "sandbox-publishable-key",
            ccNo: $("#ccNo").val(),
            cvv: $("#cvv").val(),
            expMonth: $("#expMonth").val(),
            expYear: $("#expYear").val()
        };

        // Make the token request
        TCO.requestToken(successCallback, errorCallback, args);
    };

    $(function() {
        // Pull in the public encryption key for our environment
        TCO.loadPubKey('sandbox');

        $("#myCCForm").submit(function(e) {
            // Call our token request function
            tokenRequest();

            // Prevent form from submitting
            return false;
        });
    });
</script>
```

Let's take a second to look at what we did here. First we pulled in a jQuery library to help us with manipulating the document.
(The 2co.js library does NOT require jQuery.)

Next we pulled in the 2co.js library so that we can make our token request with the card details.

```
<script src="https://www.2checkout.com/checkout/api/2co.min.js"></script>
```

This library provides us with 2 functions, one to load the public encryption key, and one to make the token request.

The `TCO.loadPubKey(String environment, Function callback)` function must be used to asynchronously load the public encryption key for the 'production' or 'sandbox' environment. In this example, we are going to call this as soon as the document is ready so it is not necessary to provide a callback.

```
TCO.loadPubKey('sandbox');
```

The the 'TCO.requestToken(Function callback, Function callback, Object arguments)' function is used to make the token request. This function takes 3 arguments:

* Your success callback function which accepts one argument and will be called when the request is successful.
* Your error callback function which accepts one argument and will be called when the request results in an error.
* An object containing the credit card details and your credentials.
    * **sellerId** : 2Checkout account number
    * **publishableKey** : Payment API publishable key
    * **ccNo** : Credit Card Number
    * **expMonth** : Card Expiration Month
    * **expYear** : Card Expiration Year
    * **cvv** : Card Verification Code

```
TCO.requestToken(successCallback, errorCallback, args);
```

In our example we created 'tokenRequest' function to setup our arguments by pulling the values entered on the credit card form and we make the token request.

```
var tokenRequest = function() {
    // Setup token request arguments
    var args = {
        sellerId: "sandbox-seller-id",
        publishableKey: "sandbox-publishable-key",
        ccNo: $("#ccNo").val(),
        cvv: $("#cvv").val(),
        expMonth: $("#expMonth").val(),
        expYear: $("#expYear").val()
    };

    // Make the token request
    TCO.requestToken(successCallback, errorCallback, args);
};
```

We then call this function from a submit handler function that we setup on the form.

```
$("#myCCForm").submit(function(e) {
    // Call our token request function
    tokenRequest();

    // Prevent form from submitting
    return false;
});
```

The 'successCallback' function is called if the token request is successful. In this function we set the token as the value for our 'token' hidden input element and we submit the form to our server.

```
var successCallback = function(data) {
    var myForm = document.getElementById('myCCForm');

    // Set the token as the value for the token input
    myForm.token.value = data.response.token.token;

    // IMPORTANT: Here we call `submit()` on the form element directly instead of using jQuery to prevent and infinite token request loop.
    myForm.submit();
};
```

The 'errorCallback' function is called if the token request fails. In our example function, we check for error code 200, which indicates that the ajax call has failed. If the error code was 200, we automatically re-attempt the tokenization, otherwise, we alert with the error message.

```
var errorCallback = function(data) {
    if (data.errorCode === 200) {
        tokenRequest();
    } else {
        alert(data.errorMsg);
    }
};
```

Next open the 'Orders/Process' view and add the HTML below.

```
<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Process</title>
</head>
<body>
    <h2><%: ViewBag.Message %></h2>
</body>
</html>
```

This view will display the transaction result to the buyer.

----

# Create the Sale

Create a new controller named Orders and add the following dependencies.

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TwoCheckout;
```

Next lets create actions for 'Index' and 'Process'.

```
public ActionResult Index()
{

}

public ActionResult Process()
{

}
```

In the 'Index' action, we will return out View 'Index.aspx' and display it to the buyer.

```
public ActionResult Index()
{
    return View();
}
```

In the 'Process' action, we will use the token passed from our credit card form to submit the authorization request and display the response.
Replace 'sandbox-seller-id' and 'sandbox-private-key' with your credentials.


```
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
```

Lets break down this action a bit and explain what were doing here.

First we setup our credentials and the environment.
* TwoCheckoutConfig.SellerID = Checkout account number
* TwoCheckoutConfig.PrivateKey = Your Payment API private key
* TwoCheckoutConfig.Sandbox = Set to true to use the sandbox (optional)

Next we create an 'AuthBillingAddress' object and a 'Customer' object. In our example we are using hard coded strings for each required attribute except for the token which is passed in from the credit card form. Notice that the 'AuthBillingAddress' object must be set as the value of the 'Customer.billingAddr' property.

**Important Note: A token can only be used for one authorization call, and will expire after 30 minutes if not used.**

```
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
```

Finally we create a new 'ChargeService' object to submit the charge using the ChargeService 'Authorize(Customer)' function and display the result to the buyer. It is important to wrap this in a try/catch block so that you can handle the response and rescue the 'TwoCheckoutException' exception that will be thrown if the card fails to authorize.

```
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
```

----

# Run the example application

Build and run the application in Visual Studio. This will open your browser to the 'Index' view where you should see a payment form to enter credit card information.

For your testing, you can use these values for a successful authorization
>Credit Card Number: 4000000000000002

>Expiration date: 10/2020

>cvv: 123

And these values for a failed authorization:

>Credit Card Number: 4333433343334333

>Expiration date: 10/2020

>cvv:123

If you have any questions, feel free to send them to techsupport@2co.com
