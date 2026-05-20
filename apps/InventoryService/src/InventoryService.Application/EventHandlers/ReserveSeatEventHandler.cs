using Exceptions;
using InventoryService.Application.Interfaces;
using MessageClient.Interfaces;
using SharedContracts.Saga.Commands;
using SharedContracts.Saga.Events;

namespace InventoryService.Application.EventHandlers;

public sealed class ReserveSeatEventHandler : IMessageHandler<ReserveSeatCommand>
{
    private readonly IMessageRepository _repository;

    public ReserveSeatEventHandler(IMessageRepository repository)
    {
        _repository    = repository;
    }
    
    public async  Task Handle(ReserveSeatCommand message, CancellationToken cancellationToken)
    {
       await  _repository.AddAsync(message,cancellationToken);
       
    }
}