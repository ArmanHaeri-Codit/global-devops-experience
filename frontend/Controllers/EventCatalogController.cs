using GloboTicket.Catalog.Controllers;
using GloboTicket.Frontend.Extensions;
using GloboTicket.Frontend.Models;
using GloboTicket.Frontend.Models.Api;
using GloboTicket.Frontend.Models.View;
using GloboTicket.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

using Event=GloboTicket.Frontend.Models.Api.Event;

namespace GloboTicket.Frontend.Controllers
{
    public class EventCatalogController : Controller
    {
        private readonly IEventCatalogService eventCatalogService;
        private readonly IShoppingBasketService shoppingBasketService;
        private readonly Settings settings;

        public EventCatalogController(IEventCatalogService eventCatalogService, IShoppingBasketService shoppingBasketService, Settings settings)
        {
            this.eventCatalogService = eventCatalogService;
            this.shoppingBasketService = shoppingBasketService;
            this.settings = settings;
        }

        public async Task<IActionResult> Index()
        {
            var currentBasketId = Request.Cookies.GetCurrentBasketId(settings);

            var getBasket = currentBasketId == Guid.Empty ? Task.FromResult<Basket>(null) :
                shoppingBasketService.GetBasket(currentBasketId);
            var getEvents = eventCatalogService.GetAll();

            await Task.WhenAll(new Task[] { getBasket, getEvents });

            var numberOfItems = getBasket.Result == null ? 0 : getBasket.Result.NumberOfItems;

            return View(
                new EventListModel
                {
                    Events = getEvents.Result,
                    NumberOfItems = numberOfItems,
                }
            );
        }

        public async Task<IActionResult> Detail(Guid eventId)
        {
            var ev = await eventCatalogService.GetEvent(eventId);
            return View(ev);
        }

        public IActionResult Create()
        {
            return View("Edit", new Event());
        }

        [HttpPost]
        public async Task<IActionResult> Recommendations([FromForm] GetRecommendations getRecommendations)
        {
            var events = await eventCatalogService.GetRecommendations(getRecommendations.Artist);
            return View("Recommendations",
                        new EventListModel
                        {
                            Events = events,
                            NumberOfItems = events.Count(),
                        });
        }

        public IActionResult Recommend()
        {
            return View();
        }
        
        public async Task<IActionResult> SaveEvent([FromForm] CreateEventRequest createEventRequest)
        {
            if (!createEventRequest.IsValid)
                return new BadRequestResult();

            await eventCatalogService.CreateEvent(createEventRequest);

            return RedirectToAction("Index");
        }
    }
}
