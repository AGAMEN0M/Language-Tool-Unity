/*
 * ---------------------------------------------------------------------------
 * Description: Handles language update notifications within the application.
 *              Provides a static delegate and event mechanism so that external
 *              components can subscribe and respond when a language change occurs.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

namespace LanguageTools
{
    /// <summary>
    /// Provides a centralized event system for language updates.
    /// </summary>
    /// <remarks>
    /// This static class exposes a delegate and an event that external systems can
    /// subscribe to in order to respond to language changes.
    /// </remarks>
    public static class LanguageManagerDelegate
    {
        /// <summary>
        /// Delegate type for language update notifications.
        /// </summary>
        /// <remarks>
        /// Methods matching this delegate can be subscribed to the language update event.
        /// </remarks>
        public delegate void LanguageUpdateDelegate();

        /// <summary>
        /// Event triggered when the application's language is updated.
        /// </summary>
        /// <remarks>
        /// Other components can subscribe to this event to be notified when a language change occurs.
        /// </remarks>
        public static event LanguageUpdateDelegate OnLanguageUpdate;

        /// <summary>
        /// Raises the language update event to notify all subscribers.
        /// </summary>
        /// <remarks>
        /// Only invokes the event if there are subscribers registered.
        /// </remarks>
        public static void NotifyLanguageUpdate() => OnLanguageUpdate?.Invoke(); // If there are subscribers to the event, invoke the delegate.
    }
}