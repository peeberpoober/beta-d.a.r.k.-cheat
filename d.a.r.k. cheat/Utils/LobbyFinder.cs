using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace dark_cheat
{
    public static class LobbyFinder
    {
        public static List<Lobby> FoundLobbies { get; private set; } = new List<Lobby>();
        public static HashSet<SteamId> AlreadyTriedLobbies = new HashSet<SteamId>();
        public static event Action OnLobbyListUpdated;
        public static Lobby SelectedLobby { get; set; }
        public static bool IsRefreshing { get; private set; }

        public static void RefreshLobbies(int maxResults = 100)
        {
            if (IsRefreshing) return;
            Debug.Log("Starting Coroutine: RefreshLobbiesCoroutine");
            Hax2.CoroutineHost.StartCoroutine(RefreshLobbiesCoroutine(maxResults));
        }

        private static IEnumerator RefreshLobbiesCoroutine(int maxResults)
        {
            IsRefreshing = true;
            FoundLobbies.Clear();
            AlreadyTriedLobbies.Clear();

            Task<Lobby[]> requestTask = SteamMatchmaking.LobbyList
                .WithMaxResults(maxResults)
                .FilterDistanceWorldwide()
                .RequestAsync();

            while (!requestTask.IsCompleted) yield return null;

            if (requestTask.IsFaulted || requestTask.Result == null)
            {
                IsRefreshing = false;
                OnLobbyListUpdated?.Invoke();
                yield break;
            }

            Lobby[] results = requestTask.Result;
            Debug.Log($"[LobbyFinder] Found {results.Length} lobbies.");
            FoundLobbies.AddRange(results);

            yield return Hax2.CoroutineHost.StartCoroutine(FakeJoinAndFetchLobbies(results));

            IsRefreshing = false;
            OnLobbyListUpdated?.Invoke();
        }

        private static IEnumerator FakeJoinAndFetchLobbies(Lobby[] lobbies)
        {
            int maxJoins = 50;
            int joinCount = 0;
            float timeout = 5f;

            foreach (Lobby lobby in lobbies)
            {
                if (AlreadyTriedLobbies.Contains(lobby.Id)) continue;
                AlreadyTriedLobbies.Add(lobby.Id);

                Hax2.LobbyHostCache[lobby.Id] = "Fetching...";

                Task<RoomEnter> joinTask = lobby.Join();

                float elapsed = 0f;

                while (!joinTask.IsCompleted && elapsed < timeout)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                if (!joinTask.IsCompleted)
                {
                    Debug.LogWarning("[LobbyFinder] Join timed out for lobby: " + lobby.Id);
                    continue;
                }

                if (joinTask.Result == RoomEnter.Success)
                {
                    string hostName = lobby.Owner.Name;
                    ulong hostId = lobby.Owner.Id.Value;

                    // Skip invalid lobbies (SteamID = 0 or empty host name)
                    if (hostId == 0 || string.IsNullOrWhiteSpace(hostName))
                    {
                        Debug.LogWarning("[LobbyFinder] Skipping invalid lobby: " + lobby.Id);
                        lobby.Leave();
                        continue;
                    }

                    string steamId = lobby.Owner.Id.ToString();
                    Hax2.LobbyHostCache[lobby.Id] = $"{hostName} ({steamId})";

                    List<string> memberList = new List<string>();
                    foreach (Friend member in lobby.Members)
                    {
                        string name = string.IsNullOrWhiteSpace(member.Name) ? "Unknown" : member.Name;
                        memberList.Add($"{name} ({member.Id})");
                    }

                    memberList.RemoveAll(m => m.Contains(SteamClient.Name) || m.Contains(SteamClient.SteamId.ToString()));
                    Hax2.LobbyMemberCache[lobby.Id] = memberList;

                    lobby.Leave();
                }
                else
                {
                    Hax2.LobbyHostCache[lobby.Id] = $"Failed ({lobby.Owner.Id})";
                }

                joinCount++;
                if (joinCount >= maxJoins)
                    break;

                yield return new WaitForSeconds(0.15f);
            }

            System.GC.Collect();
            Debug.Log("[LobbyFinder] Finished fake joining all lobbies.");
        }

        public static async void JoinLobbyAndPlay(Lobby lobby)
        {
            try
            {
                Debug.Log($"[JoinLobby] Attempting to join: {lobby.Id}");
                RoomEnter result = await lobby.Join();

                if (result == RoomEnter.Success)
                {
                    Debug.Log("[JoinLobby] Successfully joined lobby.");

                    MenuManager.instance.PageCloseAll();
                    MenuManager.instance.PageOpen(MenuPageIndex.Main, false);

                    if (RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu)
                    {
                        foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
                        {
                            player.OutroStartRPC();
                        }

                        typeof(RunManager)
                            .GetField("lobbyJoin", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.SetValue(RunManager.instance, true);

                        RunManager.instance.ChangeLevel(true, false, RunManager.ChangeLevelType.LobbyMenu);
                    }

                    typeof(SteamManager)
                        .GetField("joinLobby", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(SteamManager.instance, true);
                }
                else
                {
                    Debug.LogError("[JoinLobby] Failed to join lobby.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[JoinLobby] Exception: " + ex);
            }
        }
    }
}
