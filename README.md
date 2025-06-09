Rain World mod

TODO:
- Bugs:
    - Can wall jump while gliding off of a corner, leading to some wacky looking movement
- Add flapping behaviors
    - When gliding, pressing jump allows slugcat to flap their arms and gain some height, at the cost of reducing horizontal momentum
- Think about how grapple worms should interact with gliding/flapping
- Test gliding in zero g.
    - Initial test: seems to just not do anything in zero g
        - Maybe add zero g body mode to list of body modes that you can't start floating in
    - In low gravity, slugcat rockets upward. Probably due to lift force using player gravity not room gravity.
        - [DONE] Switch from customPlayerGravity to gravity
    - Maybe just disable the gliding physics?
    - Only allow flapping for propulsion?
- Test gliding interactions with tentacles/ropes/vines
- Add animations for gliding/flapping
    - Figure out how animation system works
        - How are the arms controlled?
    - Look into player sprites
    - Maybe can be done without sprites?
- Look into custom regions/rooms

Notes:
- Mod that makes death pits take you the rooms below: https://github.com/uzelezz123/FallThroughMap