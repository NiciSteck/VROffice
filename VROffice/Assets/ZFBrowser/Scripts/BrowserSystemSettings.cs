using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Helper class for setting options on the browser system as a whole berfore the
/// backend initalizes.
/// </summary>
public class BrowserSystemSettings : MonoBehaviour {
	[Tooltip("Where should we save the user's web profile (cookies, localStorage, etc.)? Leave blank to forget every times we restart.")]
	public string profilePath;

		[Tooltip("What user agent should we send to web pages when we request sites? Leave blank to use the default.")]
		public string userAgent; // = "Mozilla/5.0 (iPad; CPU OS 13_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/87.0.4280.77 Mobile/15E148 Safari/604.1";

	public void Awake() {
		if (!string.IsNullOrEmpty(profilePath)) {
			BrowserNative.ProfilePath = profilePath;
		}

		if (!string.IsNullOrEmpty(userAgent)) {
			Debug.Log("Setting browser user agent");
			UserAgent.SetUserAgent(userAgent);
		}
	}

}

}