import React from "react";
import { ChatUserRoleEnum } from "./models/chatUserRoleEnum";
import ReactMarkdown from "react-markdown";

interface UserMessageProps {
  message?: string;
  role: ChatUserRoleEnum;
}

const UserMessage: React.FC<UserMessageProps> = ({ message, role }) => (
  <article className="w-full" dir="auto">
    <h5 className="sr-only">Du sade:</h5>
    <div className="text-base my-auto mx-auto py-3 ">
      <div
        className={`flex w-full flex-col gap-1 empty:hidden items-end rtl:items-star ${
          role === "user" ? "items-end" : "items-start"
        }`}
      >
        <div
          className={`relative rounded-3xl px-5 py-2.5 ${
            role === "user" ? "bg-stone-200" : ""
          }`}
        >
          <div className="whitespace-pre-wrap">
            {message && <ReactMarkdown>{message}</ReactMarkdown>}
          </div>
        </div>
      </div>
    </div>
  </article>
);

export default UserMessage;
