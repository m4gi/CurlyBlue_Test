# Multiplayer With Photon Fusion!

## Resource using in Project:
### 1. Environment:
- [AnimeNaturalEnvironment](https://assetstore.unity.com/packages/3d/environments/fantasy/anime-natural-environment-236927)
### 2. Character:
- [StellarWitch 3D ](https://assetstore.unity.com/packages/3d/characters/stellarwitch-3d-309418)
### 3. Animation:
- [Female Runner Animset ](https://assetstore.unity.com/packages/3d/animations/female-runner-animset-free-walk-jog-301499)
### 4. Ads On:
- [Fusion 2 SDK & Download | Photon Engine](https://doc.photonengine.com/fusion/current/getting-started/sdk-download)
- [Fusion 2 Simple KCC | Photon Engine](https://doc.photonengine.com/fusion/current/addons/simple-kcc)
## How To Use:
- Unity Editor Version: 2022.3.40f1
- Input: Unity Input Manager
-  **Set Up Fusion App ID**:
    -   Log in to your Photon Engine dashboard.        
    -   Create a new Fusion app and copy the  **App ID**.        
    -   In Unity, go to  `Fusion > Fusion > PhotonAppSettings`.        
    -   Paste the App ID into the  `App Id Fusion`  field.
- **Test Your Game**
	-  **Local Testing**:
	    -   Run multiple instances of your game in Unity (use  `Build and Run`  for testing).        
	    -   Test hosting and joining sessions.
    - **Cloud Testing**:
	    -   Deploy your game to a build and test it on different devices.       
	    -   Use the Photon Cloud to host your game sessions.
## Photon Fusion & Using:
### **Configure Network Settings**
1.  **Network Project Config**:    
    -   Go to  `Fusion > Fusion > Network Project Config`.        
    -   Configure settings like:        
        -   **Simulation Mode**: Server/Host or Shared (Peer-to-Peer).            
        -   **Tick Rate**: Default is 60 ticks per second.            
        -   **Scene Management**: Enable auto-scene switching if needed.            
2.  **Network Runner**:    
    -   Create a new empty GameObject in your scene.        
    -   Add the  `NetworkRunner`  component to it.        
    -   Configure the  `NetworkRunner`  settings:        
        -   **Game Mode**: Host, Client, or Shared.            
        -   **Session Name**: Unique name for your game session.           
----------
### **Create Networked Objects**
1.  **Network Object**:    
    -   Create a prefab for any object you want to be networked.        
    -   Add the  `NetworkObject`  component to the prefab.        
    -   Assign a unique  `NetworkId`.        
2.  **Network Transform**:    
    -   Add the  `NetworkTransform`  component to synchronize the object's position, rotation, and scale across the network.        
3.  **Networked Scripts**:    
    -   Write scripts that inherit from  `NetworkBehaviour`  instead of  `MonoBehaviour`.        
    -   Use  `[Networked]`  attributes for variables that need to be synchronized across the network.        
----------
### **Handle Game Logic**
1.  **Spawn Players**:    
    -   Use  `NetworkRunner.Spawn()`  to instantiate player objects.        
    -   Ensure each player has a unique  `PlayerRef`.        
2.  **Input Handling**:    
    -   Use  `NetworkRunner.GetInput()`  to gather input from players.        
    -   Synchronize input data using  `[Networked]`  properties.        
3.  **State Authority**:   
    -   Use  `HasStateAuthority`  to check if the local instance has control over an object.

## Some Logic of Code
**Spaw Player - PlayerJoined**
```csharp
public void PlayerJoined(PlayerRef playerRef)
{
	if (HasStateAuthority == false)
		return;

	var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
	var spawnPosition = transform.position + new Vector3(randomPositionOffset.x, transform.position.y, randomPositionOffset.y);

	var player = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, playerRef);
	Runner.SetPlayerObject(playerRef, player);
}
```

**Despawn Player - PlayerLeft**
```csharp
public void PlayerLeft(PlayerRef playerRef)
{
	if (HasStateAuthority == false)
		return;

	var player = Runner.GetPlayerObject(playerRef);
	if (player != null)
	{
		Runner.Despawn(player);
	}
}
```