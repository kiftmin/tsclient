#pragma checksum "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5e81fcf9436005fc4b649869a114eabdfdf05891"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Backup_Index), @"mvc.1.0.view", @"/Views/Home/Backup Index.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\_ViewImports.cshtml"
using TrackingClient;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\_ViewImports.cshtml"
using TrackingClient.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 1 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
using TestClass;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
using TrackingClient.Services;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"5e81fcf9436005fc4b649869a114eabdfdf05891", @"/Views/Home/Backup Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ed25df8759b193e8da9be3e40d84b0702661fc9e", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Backup_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper;
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
  

    Layout = null;
    string UnitStatus_Default = "Blue";

    bool detected = @Global.IOConnectionStatus;
    bool reader = @Global.ReaderConnectionStatus;

    if (reader == false)
    {
        UnitStatus_Default = "";
        detected = true;
    }

    if (detected == false)
    {
        UnitStatus_Default = "";
        reader = true;
    }



#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n\r\n<!DOCTYPE html>\r\n<html>\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e81fcf9436005fc4b649869a114eabdfdf058914285", async() => {
                WriteLiteral("\r\n    <meta name=\"viewport\" content=\"width=device-width\" />\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e81fcf9436005fc4b649869a114eabdfdf058915316", async() => {
                WriteLiteral("\r\n");
#nullable restore
#line 33 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
      
        if (UnitStatus_Default == "Blue")
        {

#line default
#line hidden
#nullable disable
                WriteLiteral("            <div style=\"margin-left:auto;margin-right:auto;color:white;background-color:blue\">\r\n\r\n                <h1><b style=\"text-align:center\">Waiting For Unit</b></h1><br /><br />\r\n                <br /><br /><br /><br /><br />\r\n            </div>\r\n");
#nullable restore
#line 41 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
        }


        if (reader == false)
        {

#line default
#line hidden
#nullable disable
                WriteLiteral("            <div style=\"margin-left:auto;margin-right:auto;color:white;background-color:gold\">\r\n\r\n                <h1><b style=\"text-align:center\">Unit Detected</b></h1><br /><br />\r\n                <br /><br /><br /><br /><br />\r\n            </div>\r\n");
#nullable restore
#line 51 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
        }

        if (detected == false)
        {

#line default
#line hidden
#nullable disable
                WriteLiteral(@"            <div style=""margin-left:auto;margin-right:auto;color:white;background-color:gold"">
                <br /><br /><br /><br /><br />
                <h1>
                    <b style=""text-align:center;margin-left:auto;margin-right:auto"">
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                ");
                WriteLiteral(@"        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        TAG FOUND
                    </b>
");
                WriteLiteral("                </h1><br /><br />\r\n                <br /><br /><br /><br /><br />\r\n            </div>\r\n");
#nullable restore
#line 81 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
        }

    

#line default
#line hidden
#nullable disable
                WriteLiteral("    <hr />\r\n    <table style=\"margin-left:auto;margin-right:auto\" cellpadding=\"0\" cellspacing=\"0\">\r\n        <tr>\r\n            <td><b>Time: </b> <span>&nbsp;</span>");
#nullable restore
#line 87 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
                                            Write(Global.ReaderConnectionStatusDate.ToString("yyyy-MM-dd HH:mm:ss"));

#line default
#line hidden
#nullable disable
                WriteLiteral("</td>\r\n            <td>\r\n            </td>\r\n");
                WriteLiteral("        </tr>\r\n        <tr>\r\n            <td><b><br /><br /> Reader:  </b> <span>&nbsp;</span>");
#nullable restore
#line 103 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
                                                            Write(ViewBag.myVar2);

#line default
#line hidden
#nullable disable
                WriteLiteral(@"</td>
        </tr>

    </table>
    <h4 style=""color:gray;margin-left:auto;margin-right:auto;text-align:center""><b>Status</b></h4>
    <table style=""margin-left:auto;margin-right:auto;"" cellpadding=""0"" cellspacing=""0"">
        <tr>
            <td><b>Reader Heartbeat:</b> <span>&nbsp;</span> ");
#nullable restore
#line 110 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
                                                        Write(Global.ReaderConnectionStatus);

#line default
#line hidden
#nullable disable
                WriteLiteral("</td>\r\n\r\n        </tr>\r\n        <tr>\r\n            <td>\r\n                <span>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>\r\n                <b>  IO Heartbeat: </b> <span>&nbsp;</span> ");
#nullable restore
#line 116 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
                                                       Write(Global.IOConnectionStatus);

#line default
#line hidden
#nullable disable
                WriteLiteral("\r\n            </td>\r\n        </tr>\r\n    </table>\r\n\r\n    <h3>\r\n        ");
#nullable restore
#line 122 "C:\kiftmin\BransystemsCC\BranSystemsCloudContainers\TrackingClient\Views\Home\Backup Index.cshtml"
   Write(Html.ActionLink("Reload", "ReadTag"));

#line default
#line hidden
#nullable disable
                WriteLiteral("\r\n    </h3>\r\n");
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n</html>\r\n\r\n\r\n<script>\r\n    window.setInterval(\'refresh()\', 3000);\r\n\r\n    function refresh() {\r\n        window.location.reload();\r\n    }\r\n\r\n    function OnSuccess(response) { alert(response.d); }\r\n</script>\r\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591