# Multi-Terminal Performance Guide

Date: 2026-08-17

This guide gives a practical order for making the POS faster when you plan to run multiple terminals.

## Core rule

- Use `async/await` for I/O-bound work.
- Use `Task.Run` only for CPU-bound work that would otherwise block the UI thread.
- Do not wrap normal EF or SQL calls in `Task.Run`.

## What to optimize first

1. Keep login critical-path work as small as possible.
2. Show the main shell before non-essential data loads.
3. Remove duplicate refreshes and repeated dashboard loads.
4. Parallelize only independent reads.
5. Reduce round trips with better queries, projections, and stored procedures.
6. Add or verify database indexes for hot paths.

## Good uses of `Task.Run`

- bcrypt or other password hashing/verification
- large in-memory transformations
- expensive parsing or formatting
- CPU-heavy export/report preparation

## Bad uses of `Task.Run`

- EF Core queries
- `SqlConnection` / `SqlCommand` calls
- simple UI event handlers
- work that is already naturally asynchronous

## Multi-terminal concerns

- Keep shift open/close atomic.
- Keep transaction creation as one business command.
- Avoid shared in-memory state between terminals.
- Make concurrency behavior explicit where two terminals can touch the same record.
- Measure login and first-screen load on a cold start and a warm start.

## Performance checklist

- Login opens the shell quickly.
- Dashboard loads after the shell is visible.
- Independent sections load in parallel.
- Repeated refreshes are removed.
- Hot queries are projection-first or procedure-backed.
- Database indexes exist for the lookup columns you hit most.
- Trace logs are short enough to read during real troubleshooting.

## Practical recommendation

For a POS with multiple terminals, the biggest wins usually come from:

- shorter critical-path login
- fewer database round trips
- better query shape
- more explicit command boundaries
- measured use of background CPU work

Threads help only when they reduce blocking. They do not replace good query design or good transaction design.
