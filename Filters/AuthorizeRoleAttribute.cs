using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace QuanLyDKHP.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            var userRole = httpContext.Session["UserRole"]?.ToString();
            return userRole != null && _roles.Contains(userRole);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Lấy thông tin Area từ RouteData
            var area = filterContext.RouteData.DataTokens["area"]?.ToString();

            if (!string.IsNullOrEmpty(area) && area == "Admin")

            {
                // Chuyển hướng trong Area Admin
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "area", "Admin" },
                        { "controller", "Admin" },
                        { "action", "Login" }
                    });
            }
            else
            {
                // Chuyển hướng ngoài Area (vùng gốc)
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "area", "" },
                        { "controller", "Home" },
                        { "action", "Login" }
                    });
            }
        }
    }
}