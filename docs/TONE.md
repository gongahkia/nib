# Tone contract

**Codename:** Palimpsest Run (provisional). Naming is intentionally deferred.

## Emotional pillars

The runner is small, fast, and exposed. Every screen combines vulnerability before a pursuing hand, awe at a city whose older layers disappear beyond the frame, exhilaration from momentum, and sharp moments of physical comedy. Kinetic pressure rarely stops; relief comes from mastery and tiny reactions, not cutscenes.

## Visual hierarchy at 128×128

1. The runner uses the lightest bone and cyan values with a magenta accent.
2. Collision terrain has solid warm edges; destructible terrain is warm, stippled, and cracked.
3. Hazards use magenta; anchors and collectibles use gold; humans use pale cyan; machines use green-cyan.
4. The hand is a dark rose silhouette with bright loaded fingertips.
5. Backgrounds use only the darkest values, ordered dither, parallax, tiny windows, and occlusion.

All visible output resolves to the shared 16-colour palette. Foreground patterns are authored; runtime ordered dithering is limited to fog, light, and depth. UI may occlude scenery but not immediate collision edges.

## Coexistence

Dread is the hand gaining ground with audible finger impacts. Scale comes from slow parallax, cropped structures, distant traffic, and rooms smaller than background machinery. Speed comes from camera lead, compact trails, rapid poses, and surface audio. Playfulness comes from elastic squash, panicked inhabitants, clattering machines, collectible chirps, and exaggerated but brief recovery poses. Play never makes capture gentle or the hand friendly.

Acceptable: a resident dives under a table as a finger punches through the ceiling; a rail emits rising notes as speed builds; the runner pinwheels once after a bad landing and recovers. Unacceptable: jokes in dialogue boxes, cute facial expressions on the hand, screen-filling particles that hide hazards, solemn exposition, or pauses that halt the chase.

## Animation and sound

Silhouette precedes detail. Each gameplay state has a distinct spine angle, limb spread, and one-frame anticipation where input latency permits. Squash is at most two pixels and rotation snaps to readable increments. Important contacts receive a two-frame flash and one concise synthesized sound. Pitch follows speed; materials change timbre. Hand impacts remain lower and louder than runner sounds. No effect introduces a seventeenth colour.
