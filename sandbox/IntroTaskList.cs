namespace Sandbox;

using DisCatSharp.Entities;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

public class IntroTaskData {
	public DiscordUser Author { get; set; }
	public DiscordMessage UserMessage { get; set; }
	public Task Task { get; set; }
	public CancellationTokenSource TokenSource { get; set; }
	public DiscordMessage? StatusMessage { get; set; }

	public IntroTaskData(DiscordMessage UserMessage, Task Task, CancellationTokenSource TokenSource, DiscordMessage StatusMessage) {
		Author = UserMessage.Author;
		this.UserMessage = UserMessage;
		this.Task = Task;
		this.TokenSource = TokenSource;
		this.StatusMessage = StatusMessage;
	}

	public IntroTaskData(DiscordMessage UserMessage, Task Task, CancellationTokenSource TokenSource) {
		Author = UserMessage.Author;
		this.UserMessage = UserMessage;
		this.Task = Task;
		this.TokenSource = TokenSource;
	}
}

public class IntroTaskList:IEnumerable {
	private ulong internalHandle = 0;
	private readonly Dictionary<ulong, IntroTaskData> data = new();

	private readonly Dictionary<DiscordUser, List<ulong>> UserToInternalHandle = new();
	private readonly Dictionary<DiscordMessage, ulong> MessageToInternalHandle = new();

	public ImmutableDictionary<DiscordUser, ImmutableList<IntroTaskData>> ByUser => UserToInternalHandle.ToImmutableDictionary(
		pair => pair.Key,
		pair => pair.Value.Select(id => data[id]).ToList().ToImmutableList()
	);
	public ImmutableDictionary<DiscordMessage, IntroTaskData> ByMessage => MessageToInternalHandle.ToImmutableDictionary(pair => pair.Key, pair => data[pair.Value]);

	public int Count => data.Count;
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This class mimics ICollection<T>")]
	public bool IsReadOnly => false;



	public IntroTaskData Add(DiscordMessage userMessage, Task task, CancellationTokenSource tokenSource, DiscordMessage statusMessage) {
		lock(this) {
			IntroTaskData item = new(userMessage, task, tokenSource, statusMessage);
			data.Add(internalHandle, item);
			if(!UserToInternalHandle.ContainsKey(userMessage.Author))
				UserToInternalHandle[userMessage.Author] = new();
			UserToInternalHandle[userMessage.Author].Add(internalHandle);
			MessageToInternalHandle.Add(userMessage, internalHandle);
			internalHandle++;
			return item;
		}
	}
	public IntroTaskData Add(DiscordMessage userMessage, Task task, CancellationTokenSource tokenSource) {
		lock(this) {
			IntroTaskData item = new(userMessage, task, tokenSource);
			data.Add(internalHandle, item);
			if(!UserToInternalHandle.ContainsKey(userMessage.Author))
				UserToInternalHandle[userMessage.Author] = new();
			UserToInternalHandle[userMessage.Author].Add(internalHandle);
			MessageToInternalHandle.Add(userMessage, internalHandle);
			internalHandle++;
			return item;
		}
	}

	public void Clear() {
		lock(this) {
			data.Clear();
			UserToInternalHandle.Clear();
			MessageToInternalHandle.Clear();
			internalHandle = 0;
		}
	}

	public bool Contains(DiscordUser user) {
		lock(this) {
			return UserToInternalHandle.ContainsKey(user);
		}
	}

	public bool Contains(DiscordMessage message) {
		lock(this) {
			return MessageToInternalHandle.ContainsKey(message);
		}
	}

	public bool Remove(DiscordUser user) {
		lock(this) {
			if(UserToInternalHandle.TryGetValue(user, out List<ulong>? internalHandles)) {
				foreach(ulong handle in internalHandles) {
					data.Remove(handle);
					MessageToInternalHandle.Remove(data[handle].UserMessage);
				}
				UserToInternalHandle.Remove(user);
				return true;
			}
			return false;
		}
	}
	public bool Remove(DiscordMessage message) {
		lock(this) {
			if(MessageToInternalHandle.TryGetValue(message, out ulong internalHandle)) {
				List<ulong> handles = UserToInternalHandle[data[internalHandle].Author];
				handles.Remove(internalHandle);
				if(handles.Count == 0)
					UserToInternalHandle.Remove(data[internalHandle].Author);
				data.Remove(internalHandle);
				MessageToInternalHandle.Remove(message);
				return true;
			}
			return false;
		}
	}
	public IEnumerator<IntroTaskData> GetEnumerator() => data.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool UpdateStatusMessage(DiscordMessage message, DiscordMessage statusMessage) {
		lock(this) {
			if(MessageToInternalHandle.TryGetValue(message, out ulong internalHandle)) {
				data[internalHandle].StatusMessage = statusMessage;
				return true;
			}
			return false;
		}
	}
	public bool UpdateTask(DiscordMessage message, Task task, CancellationTokenSource source) {
		lock(this) {
			if(MessageToInternalHandle.TryGetValue(message, out ulong internalHandle)) {
				data[internalHandle].Task = task;
				data[internalHandle].TokenSource = source;
				return true;
			}
			return false;
		}
	}

	public IntroTaskData? this[DiscordMessage key] {
		get {
			lock(this) {
				return data.TryGetValue(MessageToInternalHandle[key], out IntroTaskData? v) ? v : null;
			}
		}
	}

	public ImmutableList<IntroTaskData>? this[DiscordUser key] {
		get {
			lock(this) {
				return UserToInternalHandle.TryGetValue(key, out List<ulong>? v) ? v.Select(id => data[id]).ToImmutableList() : null;
			}
		}
	}
}