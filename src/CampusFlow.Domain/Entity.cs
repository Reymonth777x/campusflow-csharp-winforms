using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Domain.Common;

public abstract class Entity
{
    public int Id { get; protected set; }
}
