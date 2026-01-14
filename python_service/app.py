from __future__ import annotations

import os
from datetime import datetime
from typing import List, Optional

from fastapi import FastAPI
from pydantic import BaseModel


class ChatMessage(BaseModel):
    role: str
    content: str


class MemoryItem(BaseModel):
    type: str
    content: str
    score: float


class EventItem(BaseModel):
    app_name: str
    window_title: Optional[str] = None
    created_at: Optional[str] = None


class ChatRequest(BaseModel):
    user_message: str
    mode: str
    recent_messages: List[ChatMessage] = []
    memories: List[MemoryItem] = []
    events: List[EventItem] = []


class SuggestionAction(BaseModel):
    type: str
    minutes: Optional[int] = None
    app: Optional[str] = None
    mode: Optional[str] = None


class Suggestion(BaseModel):
    title: str
    reason: str
    actions: List[SuggestionAction] = []


class MemoryCandidate(BaseModel):
    type: str
    content: str
    confidence: float


class ChatResponse(BaseModel):
    reply: str
    suggestions: List[Suggestion] = []
    memory_candidates: List[MemoryCandidate] = []


class LLMAdapter:
    def generate(self, request: ChatRequest) -> ChatResponse:
        raise NotImplementedError


class StubAdapter(LLMAdapter):
    def generate(self, request: ChatRequest) -> ChatResponse:
        mode = request.mode or "Work"
        greeting = {
            "Study": "Alright, let's keep the focus gentle and steady.",
            "Evening": "Let's keep it calm and light for the evening.",
        }.get(mode, "Let's keep the momentum going.")

        reply = f"{greeting} You said: {request.user_message.strip()}"
        suggestion = build_suggestion(request)
        memory_candidates = extract_memory_candidates(request.user_message)
        return ChatResponse(reply=reply, suggestions=[suggestion] if suggestion else [], memory_candidates=memory_candidates)


def build_suggestion(request: ChatRequest) -> Optional[Suggestion]:
    text = request.user_message.lower()
    if "pause" in text or "break" in text or "pause" in text:
        return Suggestion(
            title="Take a short focus break",
            reason="A quick reset keeps energy steady.",
            actions=[SuggestionAction(type="start_timer", minutes=10)],
        )
    if "notepad" in text:
        return Suggestion(
            title="Open Notepad",
            reason="You mentioned notes—want to open Notepad?",
            actions=[SuggestionAction(type="open_app", app="notepad")],
        )
    if "mode" in text and "study" in text:
        return Suggestion(
            title="Switch to Study mode",
            reason="Want me to switch your mode?",
            actions=[SuggestionAction(type="set_mode", mode="Study")],
        )
    return None


def extract_memory_candidates(message: str) -> List[MemoryCandidate]:
    lowered = message.lower()
    candidates: List[MemoryCandidate] = []
    if "i like" in lowered or "ich mag" in lowered:
        content = message.strip()
        candidates.append(MemoryCandidate(type="preference", content=content, confidence=0.7))
    return candidates


app = FastAPI()


@app.get("/health")
async def health() -> dict:
    return {"ok": True, "ts": datetime.utcnow().isoformat()}


@app.post("/chat")
async def chat(request: ChatRequest) -> ChatResponse:
    adapter: LLMAdapter
    if os.getenv("LOCAL_LLM") == "1":
        adapter = StubAdapter()
    else:
        adapter = StubAdapter()
    return adapter.generate(request)
