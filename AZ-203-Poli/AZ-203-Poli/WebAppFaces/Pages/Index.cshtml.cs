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
        private readonly IHttpClientFactory _httpFactory;
        private readonly ApiOptions _options;

        public List<string> ThumbnailImageList { get; private set; }

        public List<string> FullImageList { get; private set; }

        public IndexModel(IHttpClientFactory httpFactory, IOptions<ApiOptions> options)
        {
            _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task OnGetAsync()
        {
            var baseUrl = _options.ApiUrl;

            var imagesUrl = Flurl.Url.Combine(baseUrl, "/images/");
            var thumbsUrl = Flurl.Url.Combine(baseUrl, "/images/thumbs/");

            using var client = _httpFactory.CreateClient();
            Task<string> getFullImages = client.GetStringAsync(imagesUrl);
            Task<string> getThumbnailImages = client.GetStringAsync(thumbsUrl);

            await Task.WhenAll(getFullImages);

            string fullImagesJson = getFullImages.Result;
            IEnumerable<string> fullImagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(fullImagesJson);
            FullImageList = fullImagesList.ToList<string>();

            string thumbImagesJson = getThumbnailImages.Result;
            IEnumerable<string> thumbImagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(thumbImagesJson);
            ThumbnailImageList = thumbImagesList.ToList<string>();
        }


        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Upload != null && Upload.Length > 0)
            {
                var baseUrl = _options.ApiUrl;
                var imagesUrl = Flurl.Url.Combine(baseUrl, $"/images/create/?filename={Upload.FileName}");

                using var image = new StreamContent(Upload.OpenReadStream());
                image.Headers.ContentType = new MediaTypeHeaderValue(Upload.ContentType);
                
                using var httpClient = _httpFactory.CreateClient();
                var response = await httpClient.PostAsync(imagesUrl, image);

                response.EnsureSuccessStatusCode();
            }
            return RedirectToPage("/Index");
        }
    }
}
