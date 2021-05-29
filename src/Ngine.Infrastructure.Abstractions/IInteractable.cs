using System;

namespace Ngine.Infrastructure.Abstractions
{
    public interface IInteractable
    {
        Action FinishInteraction { get;  set; }
    }
}
