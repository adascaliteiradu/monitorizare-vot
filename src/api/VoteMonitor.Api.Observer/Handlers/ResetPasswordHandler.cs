﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using VoteMonitor.Api.Core.Services;
using VoteMonitor.Api.Observer.Commands;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Observer.Handlers
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly VoteMonitorContext _voteMonitorContext;
        private readonly ILogger _logger;
        private readonly IHashService _hash;

        public ResetPasswordHandler(VoteMonitorContext voteMonitorContext, ILogger logger, IHashService hash)
        {
            _voteMonitorContext = voteMonitorContext;
            _logger = logger;
            _hash = hash;
        }
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Entities.Observer observer = _voteMonitorContext.Observers
                    .FirstOrDefault(o => o.Phone == request.PhoneNumber && o.IdNgo == request.IdNgo);

                if (observer == null)
                    return false;

                observer.Pin = _hash.GetHash(request.Pin);

                _voteMonitorContext.Update(observer);
                await _voteMonitorContext.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError("Exception caught during resetting of Observer password for id " + request.PhoneNumber, exception);
            }

            return false;
        }
    }
}