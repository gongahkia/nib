# Messaging applications

Telegram Desktop supports its Nib files natively. Discord and Slack do not
provide equivalent first-party theme-package APIs: the fuller integrations
below use third-party loaders and can break after application updates. Inspect
the generated files before installing them and keep the original application
available for recovery.

## Discord

`discord/Nib.theme.css` is a single-file theme for BetterDiscord and Vencord.
It contains both light and dark modes, uses semantic Discord custom properties
instead of generated class names, and has no remote imports, fonts, images, or
scripts.

### BetterDiscord

Copy `discord/Nib.theme.css` into BetterDiscord's themes folder. On macOS this
is normally `~/Library/Application Support/BetterDiscord/themes`. Open
**Discord Settings → Themes**, refresh if needed, and enable **Nib**.

### Vencord

Open **Discord Settings → Vencord → Themes**, use the local themes folder or
theme upload control supplied by the installed Vencord build, and add
`discord/Nib.theme.css`. Enable the local theme and select Discord's light or
dark appearance; the same file handles both.

BetterDiscord and Vencord are independent modifications, not Discord features.
Their installation, policy, and account-risk implications are outside Nib's
control. Nib deliberately does not automate installation or modify Discord.

## Slack

### Native custom colours

This is the safer, limited option. Match Slack's base appearance to the Nib
variant, open **Preferences → Appearance → Custom theme → Import theme**, and
paste the complete contents of `slack/nib-light.txt` or
`slack/nib-dark.txt`.

Slack's current interface maps legacy ten-colour imports onto its newer design,
so this changes the workspace chrome but cannot reproduce every semantic Nib
role. The `.txt` files are still useful as a native fallback and do not patch
the app.

### Slick

`slack/nib-light.json` and `slack/nib-dark.json` target Slick's current JSON
theme catalogue. Slick is an unofficial, early-alpha Slack modification and
its own documentation warns that it may not be allowed by Salesforce. Use it
only after reviewing that project and accepting the risk.

For a source installation of Slick, copy the desired JSON file into the
checkout's `themes/` directory, rerun Slick's documented installer/build, then
select the theme under **Preferences → Slick**. Nib's JSON uses Slick's
`vars` and `sidebar` schema without raw CSS selectors, remote imports, or
assets. Nib does not install or vendor Slick.

## Telegram Desktop

`telegram/Nib Light.tdesktop-theme` and `telegram/Nib Dark.tdesktop-theme` are
native Telegram Desktop themes. In Telegram Desktop, open **Settings → Chat
background → Choose from file** and select one of them. You can also send the
file to Saved Messages and open it there.

These are plain theme files with no bundled wallpaper, so they do not need a
ZIP container and make no network requests. Telegram does not automatically
pair two custom themes; import and select the appropriate Nib variant when you
change appearance.
