using System.IO;
using System.Threading.Tasks;
using PickingListApp.DTOs;

namespace PickingListApp.Services;

public interface IPickingListParser
{
    Task<PickingListDto> Parse(Stream pdfStream);
}
