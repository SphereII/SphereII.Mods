Entity Player
-------------

The EntityPlayer SDX mod lets you over-ride the default player class, and add new functionality.

Base Features are:

- OneBlock Crouch: Allows the player to enter one-space openings
- Soft Hands: If this is set to true, then players will recieve 1 point of damage every time they hit with their bare hands

The following Properties are set on the Player's entry in the entityclasses.xml:

      <!-- Turns on one block crouch -->
      <property name="OneBlockCrouch" value="true" />

      <!-- Turns on damage when hitting things with your bare hands -->
      <property name="SoftHands" value="true" />
	  
