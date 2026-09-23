# Character assets

Put desktop character images in this folder when packaging the app.

Recommended first-pass files:
- ruka-idle.png
- ruka-talk.png
- ruka-sleep.png
- ruka-walk-1.png
- ruka-walk-2.png

Transparent PNG or WebP assets are preferred. The runtime asset loader is intentionally separate from the animation system so a future Live2D implementation can replace it without rewriting the desktop/window layer.
