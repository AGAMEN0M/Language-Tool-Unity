/*
 * ---------------------------------------------------------------------------
 * Description: This script manages language updates in the application by 
 *              providing a delegate and event mechanism. Subscribers can register to 
 *              receive notifications when a language update occurs. 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

namespace LanguageTools
{
    // Static class responsible for managing language-related updates.
    public static class LanguageManagerDelegate
    {
        public delegate void LanguageUpdateDelegate(); // Delegate declaration for handling language update events.
        public static event LanguageUpdateDelegate OnLanguageUpdate; // Event that triggers when a language update occurs.

        // Method to notify subscribers about the language update.
        public static void NotifyLanguageUpdate()
        {
            OnLanguageUpdate?.Invoke(); // If there are subscribers to the event, invoke the delegate.
        }
    }
}