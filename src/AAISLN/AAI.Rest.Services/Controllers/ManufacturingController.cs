using AAI.Core;
using AAI.Rest.Services.Data;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AAI.Rest.Services.Controllers;

[ApiController, AllowAnonymous, Route(DataRoutes.ManufacturingRoute)]
public class ManufacturingController(ILogger<ManufacturingController> logger) : ControllerBase
{
    private readonly Faker faker = new();
    
    [HttpGet]
    [Route(DataRoutes.ManufacturingGetMachineStatusesRoute)]
    [EndpointSummary("Get Machines statuses")]
    [EndpointDescription(
        "Retrieves a list of machine statuses in the manufacturing environment. ")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMachineStatusesList()
    {
        logger.LogInformation("Retrieving machine statuses list at {DateLoaded}.", DateTime.Now);
        return Ok(FakeDataGenerator.GetMachineStatuses());
    }
    
    [HttpGet]
    [Route(DataRoutes.ManufacturingGetMachinesRoute)]
    [EndpointSummary("Get Machines List")]
    [EndpointDescription(
        "Retrieves a list of machines in the manufacturing environment. " +
        "This endpoint provides basic information about each machine, such as ID, name, and location.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMachineList()
    {
        // Simulated machine data
        logger.LogInformation("Retrieving machine list from manufacturing environment at {DateLoaded}.", DateTime.Now);
        var machines = FakeDataGenerator.GenerateMachines(faker.Random.Int(5,30));
        logger.LogInformation("Generated {Count} machines.", machines.Count);
        return Ok(machines);
    }
        
    [HttpGet]
    [Route(DataRoutes.ManufacturingGetMachineStatusRoute + "/{machineId}")]
    [EndpointSummary("Get Machine Status")]
    [EndpointDescription(
        "Retrieves the current status of a specific machine in the manufacturing environment. " +
        "This endpoint provides real-time data such as operational status, temperature, and vibration levels.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMachineStatus([FromRoute]string machineId)
    {
        logger.LogInformation("Retrieving status for machine {MachineId} at {DateLoaded}.", machineId, DateTime.Now);
        var machine = FakeDataGenerator.GenerateMachine(machineId);
        return Ok(machine);
    }

    [HttpGet]
    [Route(DataRoutes.ManufacturingGetRawMaterialsRoute)]
    [EndpointSummary("Get raw materials")]
    [EndpointDescription(
        "Retrieves a list of raw materials used in the manufacturing process. " +
        "This endpoint provides information such as material ID, name, quantity, and unit of measurement.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRawMaterials()
    {
        logger.LogInformation("Retrieving raw materials from manufacturing environment at {DateLoaded}.", DateTime.Now);
        var inventoryItems = FakeDataGenerator.GenerateInventoryItems(faker.Random.Int(30, 200));
        logger.LogInformation("Generated {Count} raw materials.", inventoryItems.Count);
        return Ok(inventoryItems);
    }

    [HttpGet]
    [Route(DataRoutes.ManufacturingGetFinishedGoodsRoute)]
    [EndpointSummary("Get finished goods")]
    [EndpointDescription(
        "Retrieves a list of finished goods produced in the manufacturing environment. " +
        "This endpoint provides information such as product ID, name, quantity, and storage location.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetFinishedGoods()
    {
        logger.LogInformation("Retrieving finished goods from manufacturing environment at {DateLoaded}.", DateTime.Now);
        var finishedGoods = FakeDataGenerator.GenerateInventoryItems(faker.Random.Int(10,200),false);
        logger.LogInformation("Generated {Count} finished goods.", finishedGoods.Count);
        return Ok(finishedGoods);
    }
}