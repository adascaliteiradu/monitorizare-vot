﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VoteMonitor.Api.Core.Services;
using VoteMonitor.Api.Notification.Commands;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Notification.Handlers
{
	public class NotificationRegistrationDataHandler :
		IRequestHandler<NotificationRegistrationDataCommand, int>,
		IRequestHandler<NewNotificationCommand, int>,
		IRequestHandler<SendNotificationToAll, int>
	{
		private readonly VoteMonitorContext _context;
		private readonly IFirebaseService _firebaseService;

		public NotificationRegistrationDataHandler(VoteMonitorContext context, IFirebaseService firebaseService)
		{
			_context = context;
			_firebaseService = firebaseService;
		}

		public Task<int> Handle(NotificationRegistrationDataCommand request, CancellationToken cancellationToken)
		{
			var existingRegistration =
				_context.NotificationRegistrationData
				.FirstOrDefault(data => data.ObserverId == request.ObserverId && data.ChannelName == request.ChannelName);

			if (existingRegistration != null)
			{
				existingRegistration.Token = request.Token;
			}
			else
			{
				var notificationReg = new NotificationRegistrationData
				{
					ObserverId = request.ObserverId,
					ChannelName = request.ChannelName,
					Token = request.Token
				};

				_context.NotificationRegistrationData.Add(notificationReg);
			}

			return _context.SaveChangesAsync(cancellationToken);
		}

		public Task<int> Handle(NewNotificationCommand request, CancellationToken cancellationToken)
		{
			var targetFcmTokens = request.Recipients
					.Select(observer => _context.NotificationRegistrationData.AsQueryable().Where(regData => regData.ObserverId == int.Parse(observer))
					.First(regData => regData.ChannelName == request.Channel))
					.Select(regDataResult => regDataResult.Token)
					.ToList();

			var response = 0;

			if (targetFcmTokens.Count > 0)
				response = _firebaseService.SendAsync(request.From, request.Title, request.Message, targetFcmTokens);

			return Task.FromResult(response);
		}

		public Task<int> Handle(SendNotificationToAll request, CancellationToken cancellationToken)
		{
			var targetFcmTokens = _context.NotificationRegistrationData
				.AsNoTracking()
				.Where(x => x.ChannelName == request.Channel)
				.Select(regDataResult => regDataResult.Token)
				.ToList();

			var response = 0;

			if (targetFcmTokens.Count > 0)
				response = _firebaseService.SendAsync(request.From, request.Title, request.Message, targetFcmTokens);

			return Task.FromResult(response);
		}
	}
}