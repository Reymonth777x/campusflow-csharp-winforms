using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Domain.Common;

public sealed class DomainException(string message) : Exception(message);