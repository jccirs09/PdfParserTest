using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PickingListApp.Data;
using PickingListApp.DTOs;

namespace PickingListApp.Services;

public class PickingListService
{
    private readonly AppDbContext _dbContext;

    public PickingListService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PickingListDto?> GetPickingListByIdAsync(int id)
    {
        var pickingList = await _dbContext.PickingLists
            .AsNoTracking()
            .Include(p => p.SoldTo)
            .Include(p => p.ShipTo)
            .Include(p => p.Items)
                .ThenInclude(i => i.TagDetails)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pickingList == null ? null : ToDto(pickingList);
    }

    public async Task<PickingListDto?> GetPickingListBySoNumberAsync(string soNumber)
    {
        var pickingList = await _dbContext.PickingLists
            .AsNoTracking()
            .Include(p => p.SoldTo)
            .Include(p => p.ShipTo)
            .Include(p => p.Items)
                .ThenInclude(i => i.TagDetails)
            .FirstOrDefaultAsync(p => p.SalesOrderNumber == soNumber);

        return pickingList == null ? null : ToDto(pickingList);
    }

    public async Task<(int id, string salesOrderNumber)> SavePickingListAsync(PickingListDto dto)
    {
        var existingList = await _dbContext.PickingLists
            .Include(p => p.SoldTo)
            .Include(p => p.ShipTo)
            .Include(p => p.Items).ThenInclude(i => i.TagDetails)
            .FirstOrDefaultAsync(p => p.SalesOrderNumber == dto.SalesOrderNumber);

        if (existingList != null)
        {
            MapOntoEntity(dto, existingList);
        }
        else
        {
            existingList = ToEntity(dto);
            _dbContext.PickingLists.Add(existingList);
        }

        await _dbContext.SaveChangesAsync();
        return (existingList.Id, existingList.SalesOrderNumber);
    }

    // --- Mapping Logic ---

    private void MapOntoEntity(PickingListDto dto, PickingList entity)
    {
        entity.PrintDateTime = dto.PrintDateTime;
        entity.PickingGroup = dto.PickingGroup;
        entity.Buyer = dto.Buyer;
        entity.ShipDate = dto.ShipDate;
        entity.PurchaseOrderNumber = dto.PurchaseOrderNumber;
        entity.OrderDate = dto.OrderDate;
        entity.JobName = dto.JobName;
        entity.SalesRep = dto.SalesRep;
        entity.ShipVia = dto.ShipVia;
        entity.FobPoint = dto.FobPoint;
        entity.Route = dto.Route;
        entity.Terms = dto.Terms;
        entity.ReceivingHours = dto.ReceivingHours;
        entity.CallBeforePhone = dto.CallBeforePhone;
        entity.TotalWeightLbs = dto.TotalWeightLbs;

        MapPartyOntoEntity(dto.SoldTo, entity.SoldTo);
        MapPartyOntoEntity(dto.ShipTo, entity.ShipTo);
        UpdateItems(dto.Items, entity.Items);
    }

    private void UpdateItems(ICollection<PickingListItemDto> dtos, ICollection<PickingListItem> entities)
    {
        // Delete items that are in the entity but not in the DTO
        var dtoIds = dtos.Select(d => d.Id).ToHashSet();
        var entitiesToRemove = entities.Where(e => e.Id != 0 && !dtoIds.Contains(e.Id)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            _dbContext.Remove(entityToRemove);
        }

        // Update existing items and add new ones
        foreach (var dto in dtos)
        {
            var existingEntity = entities.FirstOrDefault(e => e.Id == dto.Id && e.Id != 0);
            if (existingEntity != null)
            {
                // Update scalar properties
                existingEntity.LineNo = dto.LineNo;
                existingEntity.Quantity = dto.Quantity;
                existingEntity.QuantityUnit = dto.QuantityUnit;
                existingEntity.QuantityStaged = dto.QuantityStaged;
                existingEntity.ItemCode = dto.ItemCode;
                existingEntity.WidthIn = dto.WidthIn;
                existingEntity.LengthIn = dto.LengthIn;
                existingEntity.WeightLbs = dto.WeightLbs;
                existingEntity.Description = dto.Description;
                existingEntity.ProcessType = dto.ProcessType;

                UpdateTagDetails(dto.TagDetails, existingEntity.TagDetails);
            }
            else
            {
                entities.Add(ToEntity(dto));
            }
        }
    }

    private void UpdateTagDetails(ICollection<ItemTagDetailDto> dtos, ICollection<ItemTagDetail> entities)
    {
        var dtoIds = dtos.Select(d => d.Id).ToHashSet();
        var entitiesToRemove = entities.Where(e => e.Id != 0 && !dtoIds.Contains(e.Id)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            _dbContext.Remove(entityToRemove);
        }

        foreach (var dto in dtos)
        {
            var existingEntity = entities.FirstOrDefault(e => e.Id == dto.Id && e.Id != 0);
            if (existingEntity != null)
            {
                // Update scalar properties
                existingEntity.TagNo = dto.TagNo;
                existingEntity.HeatNo = dto.HeatNo;
                existingEntity.MillRef = dto.MillRef;
                existingEntity.Qty = dto.Qty;
                existingEntity.ThicknessIn = dto.ThicknessIn;
                existingEntity.Size = dto.Size;
                existingEntity.Location = dto.Location;
            }
            else
            {
                entities.Add(ToEntity(dto));
            }
        }
    }

    private void MapPartyOntoEntity(PartyDto dto, Party entity)
    {
        entity.Name = dto.Name;
        entity.Email = dto.Email;
        entity.AddressLine = dto.AddressLine;
        entity.City = dto.City;
        entity.Province = dto.Province;
        entity.PostalCode = dto.PostalCode;
    }

    private PickingList ToEntity(PickingListDto dto) => new()
    {
        SalesOrderNumber = dto.SalesOrderNumber,
        PrintDateTime = dto.PrintDateTime,
        PickingGroup = dto.PickingGroup,
        Buyer = dto.Buyer,
        ShipDate = dto.ShipDate,
        PurchaseOrderNumber = dto.PurchaseOrderNumber,
        OrderDate = dto.OrderDate,
        JobName = dto.JobName,
        SalesRep = dto.SalesRep,
        ShipVia = dto.ShipVia,
        SoldTo = ToEntity(dto.SoldTo),
        ShipTo = ToEntity(dto.ShipTo),
        FobPoint = dto.FobPoint,
        Route = dto.Route,
        Terms = dto.Terms,
        ReceivingHours = dto.ReceivingHours,
        CallBeforePhone = dto.CallBeforePhone,
        TotalWeightLbs = dto.TotalWeightLbs,
        Items = dto.Items.Select(ToEntity).ToList()
    };

    private Party ToEntity(PartyDto dto) => new()
    {
        Name = dto.Name,
        Email = dto.Email,
        AddressLine = dto.AddressLine,
        City = dto.City,
        Province = dto.Province,
        PostalCode = dto.PostalCode
    };

    private PickingListItem ToEntity(PickingListItemDto dto) => new()
    {
        LineNo = dto.LineNo,
        Quantity = dto.Quantity,
        QuantityUnit = dto.QuantityUnit,
        QuantityStaged = dto.QuantityStaged,
        ItemCode = dto.ItemCode,
        WidthIn = dto.WidthIn,
        LengthIn = dto.LengthIn,
        WeightLbs = dto.WeightLbs,
        Description = dto.Description,
        ProcessType = dto.ProcessType,
        TagDetails = dto.TagDetails.Select(ToEntity).ToList()
    };

    private ItemTagDetail ToEntity(ItemTagDetailDto dto) => new()
    {
        TagNo = dto.TagNo,
        HeatNo = dto.HeatNo,
        MillRef = dto.MillRef,
        Qty = dto.Qty,
        ThicknessIn = dto.ThicknessIn,
        Size = dto.Size,
        Location = dto.Location
    };

    private PickingListDto ToDto(PickingList entity) => new()
    {
        Id = entity.Id, SalesOrderNumber = entity.SalesOrderNumber, PrintDateTime = entity.PrintDateTime,
        PickingGroup = entity.PickingGroup, Buyer = entity.Buyer, ShipDate = entity.ShipDate,
        PurchaseOrderNumber = entity.PurchaseOrderNumber, OrderDate = entity.OrderDate, JobName = entity.JobName,
        SalesRep = entity.SalesRep, ShipVia = entity.ShipVia, FobPoint = entity.FobPoint, Route = entity.Route,
        Terms = entity.Terms, ReceivingHours = entity.ReceivingHours, CallBeforePhone = entity.CallBeforePhone,
        TotalWeightLbs = entity.TotalWeightLbs, SoldTo = ToDto(entity.SoldTo), ShipTo = ToDto(entity.ShipTo),
        Items = entity.Items.Select(ToDto).ToList()
    };

    private PartyDto ToDto(Party entity) => new()
    {
        Id = entity.Id, Name = entity.Name, Email = entity.Email, AddressLine = entity.AddressLine, City = entity.City,
        Province = entity.Province, PostalCode = entity.PostalCode
    };

    private PickingListItemDto ToDto(PickingListItem entity) => new()
    {
        Id = entity.Id, LineNo = entity.LineNo, Quantity = entity.Quantity, QuantityUnit = entity.QuantityUnit,
        QuantityStaged = entity.QuantityStaged, ItemCode = entity.ItemCode, WidthIn = entity.WidthIn,
        LengthIn = entity.LengthIn, WeightLbs = entity.WeightLbs, Description = entity.Description,
        ProcessType = entity.ProcessType, TagDetails = entity.TagDetails.Select(ToDto).ToList()
    };

    private ItemTagDetailDto ToDto(ItemTagDetail entity) => new()
    {
        Id = entity.Id, TagNo = entity.TagNo, HeatNo = entity.HeatNo, MillRef = entity.MillRef, Qty = entity.Qty,
        ThicknessIn = entity.ThicknessIn, Size = entity.Size, Location = entity.Location
    };
}
