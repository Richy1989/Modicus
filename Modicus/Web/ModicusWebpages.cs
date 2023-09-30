using nanoFramework.WebServer;

namespace Modicus.Web
{
    public class ModicusWebpages
    {
        /// <summary>
        /// Serves the favicon
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("favicon.ico")]
        public void Favico(WebServerEventArgs e)
        {
            // WebServer.SendFileOverHTTP(e.Context.Response, "favico.ico", Resources.GetBytes(Resources.BinaryResources.favico), "image/ico");
        }

        [Route("style.css")]
        public void Style(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/css";
            WebServer.OutPutStream(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.style));
        }

        /// <summary>
        /// Serves the SVG image
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("image.svg")]
        public void Image(WebServerEventArgs e)
        {
            //  WebServer.SendFileOverHTTP(e.Context.Response, "image.svg", Resources.GetBytes(Resources.BinaryResources.image), "image/svg+xml");
        }

        /// <summary>
        /// This is the default page
        /// </summary>
        /// <remarks>the / route *must* always be the last one and the last of the last controller passed
        /// to the constructor</remarks>
        /// <param name="e">Web server context</param>
        [Route("default.html"), Route("index.html"), Route("/")]
        public void Default(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/html";

            var status_message = "Welcome to Modicus ... Have fun!";
            var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.index), status_message);
            WebServer.OutPutStream(e.Context.Response, page);
        }
    }
}