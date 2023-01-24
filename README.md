# Natural Disasters Overhaul mod

Mod for Cities: Skylines

With this mod natural disasters occur in more natural way, depending on weather, season, or time passed since the last disaster.


## Forest Fire

Features
* Do not occur during rain.
* Probability gradually increases during dry weather.

Unlocks
* From the beginning (can occur only outside of your unlocked areas)
* From Milestone 3: Worthy Village (can occur everywere, both inside and outside of your unlocked areas)

Default settings
* Warmup period: 180 days without rain
* Peak probability: 10 times/year


## Thunderstorm

Features
* Occurs mainly in the thunderstorm season.
* Probability is higher during rain.

Unlocks
* From the beginning (can occur only outside of your unlocked areas)
* From Milestone 3: Worthy Village (can occur everywere, both inside and outside of your unlocked areas)

Default settings
* Thunderstorm season peak: July
* Probability in the season peak: 2 times/year
* Probability during rain: x2


## Sinkhole

Features
* Occurs during a long rainfall and for a short time after the rain (like landslides).
* The longer and heavier the rainfall, the higher the probability. Imagine an underground reservour which is filling up during rain and emptying after rain stops.

Unlocks
* From Milestone 4: Tiny Town (can occur only inside of your unlocked areas)

Default settings
* Maximum probability after a long heavy rainfall: 1.5 times/year
* Underground reservour capacity: 50 days of heavy rainfall


## Tornado

Features
* Occurs mainly in the tornado season.
* Do not occur during fog (when the wind is weak).
* Cannot occur too often (there is a calm period after every tornado).
* Small tornadoes occur more often than big ones.
* New destruction system (see below).

Unlocks
* From Milestone 5: Boom Town (can occur only inside of your unlocked areas)

Default settings
* Tornado season peak: May
* Probability in the season peak: 1.5 times/year
* Calm period after a big tornado: 2 years (depends on the intensity of the last tornado)


## Tsunami

Features
* Probability slowly increases with time.
* Cannot occur too often (there is a long calm period after every tsunami).
* Small tsunamies occur more often than big ones.

Unlocks
* From Milestone 5: Boom Town

Default settings
* Maximum probability after a long time: 1 times/year
* Charge period during wich probability increases: 4 years
* Calm period after a big tsunami: 4 years (depends on the intensity of the last tsunami)


## Earthquake

Features
* Probability slowly increases with time.
* Cannot occur too often (there is a long calm period after each earthquake), excluding aftershocks.
* Small earthquakes occur more often than big ones (Gutenberg–Richter law distribution for earthquakes is used).
* One or several aftershocks can occur after a big earthquake (may take several months to calm down).
* Aftershocks strike the same place as the main earthquake.
* Maximum intensity of every following aftershock is weaker than the previous one.
* No cracks appear (too destructive).
* New destruction system (see below).

Unlocks
* From Milestone 6: Busy Town (can occur only inside of your unlocked areas)

Default settings
* Maximum probability after a long time: 1 times/year
* Charge period during which probability increases: 3 years
* Calm period after a big earthquake: 3 years (depends on the intensity of the last earthquake)
* Aftershocks enabled


## Meteor Strike

Features
* There are three meteoroid streams periodically approaching the Earth.
  * Long-periodical meteoroid stream: period 9 years, maximum size 100
  * Medium-periodical meteoroid stream: period 5 years, maximum size 70
  * Short-periodical meteoroid stream: period 2 years, maximum size 30
* When a meteoroid stream approaches the Earth, there is a chance of a meteor strike.

Unlocks
* From Milestone 6: Busy Town (can occur only inside your unlocked areas)

Default settings
* Peak probability when a meteoroid stream is approaching: 10 times/year (per meteoroid stream).


## About new destruction system

(Implemented for Earthquake and Tornado disasters)

* Buildings of high levels or big size are more robust and have less probability to collapse.
* Very large buildings do not collapse ever.
* Evacuating buildings makes collapse probability even less (this makes shelters a little more useful).
* Evacuated residential buildings of level 5 or commercial/industry buildings of level 3 do not collapse ever.


## Disasters info panel

Shows the current probabilities and maximum intesity values for all disasters. Some detailed info can be seen in toolips by mouseover.

Also contains the Emergency Button (see below).

Show / hide the disasters info panel by clicking the "Lightning" button at the top-left of the screen, or use the key shortcut Shift+D.


## Emergency Button

One of the most annoying thing about disasters is that they soullessly destroy the world you wholeheartedly created. To save your most valuable creation from destruction, just press the Emergency Button at the bottom of the disasters info panel - this stops all currently occuring disasters, except tsunami waves.

In the current version, there are no restrictions how many times you can press the Emergency Button.


## Logging

The mod logs out information about all occured disasters into csv file in the CS data directory (C:\Users\%Username%\AppData\Colossal Order\Cities_Skylines\Disasters.csv).
* date
* name
* intensity
* reason (mod or vanilla)

Can be turned on/off in the mod options.


## Minor features
* Maximum intensity of all disasters is set to the minimum (10) at the beginning of the game and gradually increases up to the maximum (100) until the city population reaches 20000.
* Duration of small intensity thunderstorms is decreased.
* Meteoroid streams period and phase (next approaching time) are randomized at the beginning of the game.

All settings are stored in your game save file, so you can use different settings for each game.
