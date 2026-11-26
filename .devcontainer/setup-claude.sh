#!/bin/bash
# One-time setup inside the container: install the Gentle AI stack for Claude Code.
# Idempotent. Requires the gentle-ai and engram binaries (see compose.override.example.yaml).
set -euo pipefail

command -v gentle-ai >/dev/null || { echo "gentle-ai not found: create compose.override.yaml from the example." >&2; exit 1; }

gentle-ai install --agent claude-code --preset full-gentleman --persona gentleman

# gentle-ai writes MCP servers to ~/.claude.json, but with CLAUDE_CONFIG_DIR set Claude reads
# $CLAUDE_CONFIG_DIR/.claude.json, which also lives in the claude-home volume. Merge them.
node -e '
const fs = require("fs");
const src = process.env.HOME + "/.claude.json";
const dst = (process.env.CLAUDE_CONFIG_DIR || process.env.HOME + "/.claude") + "/.claude.json";
if (!fs.existsSync(src)) process.exit(0);
const s = JSON.parse(fs.readFileSync(src, "utf8"));
const d = fs.existsSync(dst) ? JSON.parse(fs.readFileSync(dst, "utf8")) : {};
d.mcpServers = { ...(d.mcpServers || {}), ...(s.mcpServers || {}) };
fs.writeFileSync(dst, JSON.stringify(d, null, 2), { mode: 0o600 });
console.log("MCP servers in", dst + ":", Object.keys(d.mcpServers).join(", "));
'
