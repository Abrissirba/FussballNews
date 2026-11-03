"use client";

import { useState } from "react";
import { fussballApiFetch } from "./api/fussballnews/fussballApiFetch";
import UserMessage from "./UserMessage";
import { ChatMessage } from "./models/chatMessage";
import { ChatUserRoleEnum } from "./models/chatUserRoleEnum";

export default function Home() {
  const [newMessage, setNewMessage] = useState("");
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setNewMessage(e.target.value);
  };

  const sendMessage = async () => {
    const userMessage = { role: ChatUserRoleEnum.User, content: newMessage };
    setMessages(prevMessages => [...prevMessages, userMessage]);
    const data = await fussballApiFetch("/api/chat", {
      method: "POST",
      json: {
        message: newMessage
      }
    });
    setNewMessage("");
    setMessages(prevMessages => [...prevMessages, { role: ChatUserRoleEnum.Assistant, content: data.message }]);
  }

  return (
    <main className="flex flex-col gap-[32px] row-start-2 items-center sm:items-start">
      <div className="w-full max-w-4xl flex flex-col gap-6">
        {/* Chat Messages Container */}
        <div className="flex basis-auto flex-col grow overflow-hidden">
          <div className="relative h-full">
            <div className="flex h-full flex-col overflow-y-auto [scrollbar-gutter:stable_both-edges] @[84rem]/thread:pt-(--header-height)">
                <div className="flex flex-col pb-25 text-sm">
                  {messages.map((message, index) => (
                    <div key={index}>
                      { <UserMessage message={message.content} role={message.role}/>}
                    </div>
                  ))}
              </div>
            </div>
          </div>
        </div>

        <form>
          <div className="rounded-3xl p-2.5 border border-stone-200">
              <textarea
                className="w-full p-2.5 resize-none focus:outline-none"
                name="prompt-textarea"
                autoFocus={true}
                placeholder="Fråga vad som helst"
                onChange={handleChange}
                value={newMessage}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    sendMessage();
                  }
                }}
              />
          </div>
        </form>
      </div>
    </main>
  );
}
