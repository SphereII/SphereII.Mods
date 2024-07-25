#if WORLDAPI_PRESENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WAPI;


namespace GlobalSnowEffect {
				/// <summary>
				/// Support for World Manager API
				/// </summary>
				public class GlobalSnowWMAPI : MonoBehaviour, IWorldApiChangeHandler {
								//Initialization
								void OnEnable () {
												ConnectToWorldAPI ();
								}

								//Disconnect from WAPI
								void OnDisable () {
												DisconnectFromWorldAPI ();
								}

								#region World Manager API Integration

								/// <summary>
								/// Start listening to world api messaged
								/// </summary>
								void ConnectToWorldAPI () {
												WorldManager.Instance.AddListener (this);
								}

								/// <summary>
								/// Stop listening to world api messages
								/// </summary>
								void DisconnectFromWorldAPI () {
												WorldManager.Instance.RemoveListener (this);
								}

								/// <summary>
								/// This method is called when the class has been added as a listener, and something has changed 
								/// one of the WAPI settings.
								/// 
								/// Use the HasChanged method to work out what was changed and respond accordingly. 
								/// 
								/// NOTE : As the majority of the World API values are properties, setting something 
								/// is as easy as reading its value, and setting a property will cause another
								/// OnWorldChanged event to be raised.
								/// 
								/// </summary>
								/// <param name="changeArgs"></param>
								public void OnWorldChanged (WorldChangeArgs changeArgs) {
												if (changeArgs.HasChanged (WorldConstants.WorldChangeEvents.SnowChanged) ||
												    changeArgs.HasChanged (WorldConstants.WorldChangeEvents.SeasonChanged)) {
																GlobalSnow.instance.minimumAltitude = WorldManager.Instance.SnowMinHeight + 125f * (2f - Mathf.Abs (2f - WorldManager.Instance.Season));
																GlobalSnow.instance.UpdateSnowfallProperties();
												}
								}

								#endregion
				}
}
#endif
