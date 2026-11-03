import { ChatUserRoleEnum } from "./chatUserRoleEnum";

export interface ChatMessage {
  role: ChatUserRoleEnum;
  content: string;
}