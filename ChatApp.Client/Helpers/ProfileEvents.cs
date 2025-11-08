using System;

namespace ChatApp.Client.Helpers;

public static class ProfileEvents
{
    public static event Action<Guid, string>? ProfileUpdated;

    public static void RaiseProfileUpdated(Guid userId, string displayNameOrUsername)
        => ProfileUpdated?.Invoke(userId, displayNameOrUsername);
}

