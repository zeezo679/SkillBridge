using AutoMapper;
using Org.BouncyCastle.Utilities;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Services.Specification.NotificationSpecification;
using SoftBridge.Shared.Common.Dto.Chat;
using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params;
using SoftBridge.Shared.Common.Params.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.NotificationImplementation
{
    public class NotificationService(IEnumerable<INotificationStrategy> _notificationStrategies , IUnitOfWork unitOfWork,IMapper mapper) : INotificationService
    {
        public async Task SendNotificationAsync(NotificationContentDto message, params NotificationType[] types)
        {
            // create the entity and save to database
            var notificationEntity = new Notification
            {
                UserId = message.UserId, 
                Title = message.Subject,
                Message = message.Body,
                ReferenceId = message.ReferenceId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            // get the repo and save the notification

            var repository = unitOfWork.GetRepository<Notification, Guid>();
            await repository.AddAsync(notificationEntity);
            await unitOfWork.SaveChangesAsync();

            // Loop through the requested types and deliver
            foreach (var type in types)
            {
                var strategy = _notificationStrategies.FirstOrDefault(s => s.Type == type);
                if (strategy != null)
                {
                    await strategy.DeliverAsync(message);
                }
            }
        }
        
        public async Task<PaginationResponse<NotificationDto>> GetUserNotificationsAsync(string userId , NotificationQueryParams queryParams)
        {
            var repo = unitOfWork.GetRepository<Notification, Guid>();

            var spec = new NotificationsByUserIdSpec(userId, queryParams);
            var countSpec = new NotificationsCountByUserIdSpec(userId,queryParams);

            var notifications = await repo.GetAllWithSpecAsync(spec);
            var totalCount = await repo.GetCountAsync(countSpec);

            var mappedData = mapper.Map<IReadOnlyList<NotificationDto>>(notifications);

            return new PaginationResponse<NotificationDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                totalCount,
                mappedData);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var repo = unitOfWork.GetRepository<Notification, Guid>();

            var unreadSpec = new UnreadNotificationsSpec(userId);
            var unreadNotifications = await repo.GetAllWithSpecAsync(unreadSpec);

            if (unreadNotifications == null || !unreadNotifications.Any())
                return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                repo.Update(notification);
            }

            return await unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, string userId)
        {
            var repo = unitOfWork.GetRepository<Notification, Guid>();
            var notification = await repo.GetByIdAsync(notificationId);

            if (notification == null)
                throw new NotificationNotFound("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedExceptionCusotme("You are not authorized to update this notification.");

            if (notification.IsRead)
                return true;

            notification.IsRead = true;
            repo.Update(notification);

            return await unitOfWork.SaveChangesAsync() > 0;
        }

    }
}
