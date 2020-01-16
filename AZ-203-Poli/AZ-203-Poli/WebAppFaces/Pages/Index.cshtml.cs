using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebAppFaces.Insfrastructure;

namespace WebAppFaces.Pages
{
    public class IndexModel : PageModel
    {
        private HttpClient _httpClient;
        private ApiOptions _options;

        public List<string> ThumbnailImageList { get; private set; }

        public List<string> FullImageList { get; private set; }

        public IndexModel(HttpClient httpClient, IOptions<ApiOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task OnGetAsync()
        {
            var baseUrl = _options.ApiUrl;

            var imagesUrl = Flurl.Url.Combine(baseUrl, "/images/");
            var thumbsUrl = Flurl.Url.Combine(baseUrl, "/thumbs/");

            Task<string> getFullImages = _httpClient.GetStringAsync(imagesUrl);
            Task<string> getThumbnailImages = _httpClient.GetStringAsync(thumbsUrl);
            await Task.WhenAll(getFullImages);

            string fullImagesJson = getFullImages.Result;
            IEnumerable<string> fullImagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(fullImagesJson);
            this.FullImageList = fullImagesList.ToList<string>();

            string thumbImagesJson = getThumbnailImages.Result;
            IEnumerable<string> thumbImagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(thumbImagesJson);
            this.ThumbnailImageList = thumbImagesList.ToList<string>();
        }


        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Upload != null && Upload.Length > 0)
            {
                var baseUrl = _options.ApiUrl;
                var imagesUrl = Flurl.Url.Combine(baseUrl, "/images/");

                using (var image = new StreamContent(Upload.OpenReadStream()))
                {
                    image.Headers.ContentType = new MediaTypeHeaderValue(Upload.ContentType);
                    var response = await _httpClient.PostAsync(imagesUrl, image);
                }
            }
            return RedirectToPage("/Index");
        }
    }
}
