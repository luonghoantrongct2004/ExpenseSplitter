import { cn } from "@/lib/utils";

interface LoadingProps {
  size?: "sm";
  className?: string;
  fullScreen?: boolean;
  showHamster?: boolean;
}

export function Loading({
  size = "sm",
  fullScreen = false,
  showHamster = true,
}: LoadingProps) {
  const sizes = {
    sm: "text-xs",
  };

  const loader = showHamster ? (
    <div
      aria-label="Orange and tan hamster running in a metal wheel"
      role="img"
      className={cn("wheel-and-hamster sm")}
    >
      <div className="wheel"></div>
      <div className="hamster">
        <div className="hamster__body">
          <div className="hamster__head">
            <div className="hamster__ear"></div>
            <div className="hamster__eye"></div>
            <div className="hamster__nose"></div>
          </div>
          <div className="hamster__limb hamster__limb--fr"></div>
          <div className="hamster__limb hamster__limb--fl"></div>
          <div className="hamster__limb hamster__limb--br"></div>
          <div className="hamster__limb hamster__limb--bl"></div>
          <div className="hamster__tail"></div>
        </div>
      </div>
      <div className="spoke"></div>
    </div>
  ) : (
    <div className={cn("loader", sizes[size])} data-text="Đang tải" />
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 flex items-center justify-center z-50 backdrop-blur-sm">
        {loader}
      </div>
    );
  }

  return <div className="flex items-center justify-center p-4">{loader}</div>;
}

export const HamsterLoader = (props: Omit<LoadingProps, "showHamster">) => (
  <Loading {...props} showHamster={true} />
);

export const TextLoader = (props: Omit<LoadingProps, "showHamster">) => (
  <Loading {...props} showHamster={false} />
);
