using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MagicVilla_VillaAPI.Controllers
{
    //Both of this below route code are valid. Can use any one of it for controller

    //this below code for route is not a good practise to use it in real time application because what if you have to change the name of the file for some reason and you have many consumer that uses this api, then you have to tell everyone of them about this change. So instead of using [controller] for route, use the name of the controller. So even if you change the file name, your route doesnot change.

    // [Route("api/[controller]")]

    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogger<VillaAPIController> _logger;

        //Implementing Logger using dependency injection as logger is already registered for this application we just need to implement it


        public VillaAPIController(ILogger<VillaAPIController> logger)
        {
            _logger = logger;
        }
         
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {

            _logger.LogInformation("Getting all the Villas");
            return Ok(VillaStore.villaList);
        }

        // using HttpGet on both the endPoints, it throws error which says there is a confusion here on which api to call because of the definition here. there is 2 HTTPGET calls. and to resolve this error/confusion we have to explicitely define that below HTTPGET requires ID as parameter.
        [HttpGet("{id:int}", Name = "GetVilla")]

        //this is hard coded status code 200,400,400

        // [ProducesResponseType(200)] // to docuement the possible response type.
        // [ProducesResponseType(400)]
        // [ProducesResponseType(404)]

        //Another way to write the status code

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0) {
                _logger.LogError("Get Villa error with Id: " + id);

                return BadRequest(); 
            
            }

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);

            if (villa == null) { return NotFound(); }

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        //when we are working with HTTPPOST, typically the object we recieve here is [fromBody]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            if (villaDTO == null) { return BadRequest(villaDTO); }

            if (villaDTO.Id > 0) { return StatusCode(StatusCodes.Status500InternalServerError); }


            //Custom Validation and how to add that in Model State
            if (VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa Already Exists!");
                return BadRequest(ModelState);
            }

            villaDTO.Id = VillaStore.villaList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

            VillaStore.villaList.Add(villaDTO);

            //return Ok(villaDTO);

            //to get the route where the data is create we use 'CreatedAtRoute'
            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
        }


        [HttpPost("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0) { return BadRequest(); }

            var villa=VillaStore.villaList.FirstOrDefault(x=>x.Id==id);

            if (villa == null)
            {
                return NotFound();
            }

            VillaStore.villaList.Remove(villa);
            
            //when we delete, we donot return anything so we write NoContent, also 'return Ok' is still valid, nothing wrong in doing that as well
            return NoContent();
        }


        [HttpPut("{id:int}", Name = "UpdateVilla")]
         
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        public IActionResult UpdateVilla(int id,[FromBody]VillaDTO villaDto)
        {
            if(villaDto == null || id != villaDto.Id)
            {
                return BadRequest();
            }


            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);

            villa.Name = villaDto.Name;
            villa.Sqft= villaDto.Sqft;
            villa.Occupancy = villaDto.Occupancy;

            return NoContent();

        }


        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchVillaDTO)
        {

            if (patchVillaDTO == null || id == 0) { return BadRequest(); }

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            patchVillaDTO.ApplyTo(villa, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }

    }
}
