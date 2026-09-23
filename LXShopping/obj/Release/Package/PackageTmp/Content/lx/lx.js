/* LX 优选商城前台的交互脚本
   需要 jQuery 3。轮播、下拉菜单、弹窗这几个用的是 Bootstrap 自带的，其它都是自己写的 */
(function (window, $) {
    "use strict";

    var LX = window.LX || {};

    /* 右上角飘一下就消失的提示框 */
    function toastContainer() {
        var $box = $(".lx-toasts");
        if (!$box.length) {
            $box = $('<div class="lx-toasts"></div>').appendTo(document.body);
        }
        return $box;
    }

    var ICONS = {
        success: '<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M8 12.4l2.6 2.6L16 9.6"/></svg>',
        error: '<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M15 9l-6 6M9 9l6 6"/></svg>',
        warning: '<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 4l9 16H3z"/><path d="M12 10v4"/><circle cx="12" cy="17.4" r="0.9" fill="currentColor" stroke="none"/></svg>',
        info: '<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M12 11v6"/><circle cx="12" cy="7.6" r="1" fill="currentColor" stroke="none"/></svg>'
    };

    var TITLES = { success: "操作成功", error: "出错了", warning: "请注意", info: "提示" };

    LX.toast = function (message, type, title) {
        if (!message) return;
        type = type || "info";
        var $t = $('<div class="lx-toast ' + type + '"></div>');
        $t.append('<span class="ic">' + (ICONS[type] || ICONS.info) + "</span>");
        $t.append('<div class="bd"><b>' + (title || TITLES[type] || "提示") + "</b><span></span></div>");
        $t.find(".bd span").text(message);
        toastContainer().append($t);

        var timer = window.setTimeout(close, type === "error" ? 5200 : 3600);
        $t.on("click", close);
        function close() {
            window.clearTimeout(timer);
            $t.addClass("hide");
            window.setTimeout(function () { $t.remove(); }, 240);
        }
        return $t;
    };

    /* 几个小工具方法 */
    LX.post = function (url, data) {
        return $.ajax({
            url: url,
            type: "POST",
            data: data || {},
            dataType: "json"
        });
    };

    LX.updateCartBadge = function (count) {
        var $c = $(".lx-cart-count");
        if (!$c.length) return;
        if (count === undefined || count === null) return;
        $c.text(count > 99 ? "99+" : count);
        $c.removeClass("pulse");
        // 这里得让浏览器强制重新排版一下，不然动画不会重新播
        void $c[0].offsetWidth;
        $c.addClass("pulse");
    };

    LX.refreshCartBadge = function () {
        $.get("/Cart/Count", function (res) {
            if (res && res.ok) LX.updateCartBadge(res.count);
        });
    };

    /* 加入购物车 */
    function bindAddToCart() {
        $(document).on("click", "[data-add-cart]", function (e) {
            e.preventDefault();
            var $btn = $(this);
            if ($btn.data("busy")) return;

            var id = $btn.data("add-cart");
            var qty = 1;
            var $qty = $btn.closest("[data-buy-area]").find("[data-qty] input");
            if ($qty.length) qty = parseInt($qty.val(), 10) || 1;

            $btn.data("busy", true).addClass("is-disabled");
            var original = $btn.html();

            LX.post("/Cart/AddToCart", { productId: id, amount: qty })
                .done(function (res) {
                    if (res && res.ok) {
                        LX.toast(res.message || "已加入购物车", "success");
                        LX.updateCartBadge(res.data && res.data.count);

                        // 点「立即购买」的话，加完购物车还得接着跳到结算页去
                        var redirect = $btn.data("cartRedirect");
                        if (redirect) {
                            window.setTimeout(function () { window.location.href = redirect; }, 520);
                        }
                    } else if (res && res.needLogin) {
                        LX.toast(res.message, "warning", "需要登录");
                        window.setTimeout(function () { window.location.href = "/Member/Login"; }, 900);
                    } else {
                        LX.toast((res && res.message) || "加入购物车失败", "error");
                    }
                })
                .fail(function () {
                    LX.toast("网络异常，请稍后重试", "error");
                })
                .always(function () {
                    $btn.removeData("busy").removeClass("is-disabled").html(original);
                });
        });
    }

    /* 商品数量那个加减的步进器 */
    function bindQtyStepper() {
        $(document).on("click", "[data-qty] .minus", function () {
            var $wrap = $(this).closest("[data-qty]");
            var $input = $wrap.find("input");
            var min = parseInt($wrap.data("min"), 10) || 1;
            var v = (parseInt($input.val(), 10) || min) - 1;
            if (v < min) v = min;
            $input.val(v).trigger("change");
        });

        $(document).on("click", "[data-qty] .plus", function () {
            var $wrap = $(this).closest("[data-qty]");
            var $input = $wrap.find("input");
            var max = parseInt($wrap.data("max"), 10) || 99;
            var v = (parseInt($input.val(), 10) || 1) + 1;
            if (v > max) v = max;
            $input.val(v).trigger("change");
        });

        $(document).on("change blur", "[data-qty] input", function () {
            var $wrap = $(this).closest("[data-qty]");
            var $input = $(this);
            var min = parseInt($wrap.data("min"), 10) || 1;
            var max = parseInt($wrap.data("max"), 10) || 99;
            var v = parseInt($input.val(), 10);
            if (isNaN(v) || v < min) v = min;
            if (v > max) v = max;
            $input.val(v);
        });
    }

    /* 购物车页面 */
    function bindCartPage() {
        var $page = $("[data-cart-page]");
        if (!$page.length) return;

        // 改数量
        $page.on("change", "[data-cart-qty]", function () {
            var $input = $(this);
            var id = $input.data("cart-qty");
            var amount = parseInt($input.val(), 10) || 1;
            $input.prop("disabled", true);

            LX.post("/Cart/UpdateAmount", { productId: id, amount: amount })
                .done(function (res) {
                    if (res && res.ok) {
                        var d = res.data;
                        $page.find('[data-line-total="' + id + '"]').text(d.lineTotal);
                        $page.find("[data-subtotal]").text(d.subTotal);
                        $page.find("[data-shipfee]").text(d.shipFee);
                        $page.find("[data-payable]").text(d.payable);
                        LX.updateCartBadge(d.count);
                    } else {
                        LX.toast((res && res.message) || "更新失败", "error");
                        window.setTimeout(function () { window.location.reload(); }, 1200);
                    }
                })
                .fail(function () { LX.toast("网络异常，请稍后重试", "error"); })
                .always(function () { $input.prop("disabled", false); });
        });

        // 删掉一整行
        $page.on("click", "[data-cart-remove]", function () {
            var id = $(this).data("cart-remove");
            if (!window.confirm("确定要从购物车移除这件商品吗？")) return;

            LX.post("/Cart/Remove", { productId: id })
                .done(function (res) {
                    if (res && res.ok) {
                        LX.toast(res.message || "已移除", "success");
                        LX.updateCartBadge(res.data && res.data.count);
                        window.setTimeout(function () { window.location.reload(); }, 500);
                    } else {
                        LX.toast((res && res.message) || "移除失败", "error");
                    }
                })
                .fail(function () { LX.toast("网络异常，请稍后重试", "error"); });
        });

        // 清空购物车
        $page.on("click", "[data-cart-clear]", function () {
            if (!window.confirm("确定要清空购物车吗？")) return;
            LX.post("/Cart/Clear")
                .done(function (res) {
                    if (res && res.ok) {
                        LX.toast("购物车已清空", "success");
                        window.setTimeout(function () { window.location.reload(); }, 500);
                    } else {
                        LX.toast((res && res.message) || "清空失败", "error");
                    }
                })
                .fail(function () { LX.toast("网络异常，请稍后重试", "error"); });
        });
    }

    /* 结算页选收货地址那一块 */
    function bindAddressPick() {
        $(document).on("click", "[data-address-pick]", function () {
            var $item = $(this);
            $("[data-address-pick]").removeClass("is-active");
            $item.addClass("is-active");
            $item.find("input[type=radio]").prop("checked", true);

            var $form = $("#newAddressBlock");
            if ($form.length) {
                $form.stop(true, true).slideUp(180);
            }
        });

        $(document).on("click", "[data-use-new-address]", function () {
            $("[data-address-pick]").removeClass("is-active");
            $("[data-address-pick] input[type=radio]").prop("checked", false);
            $("[data-address-radio-new]").prop("checked", true);
            $("#newAddressBlock").stop(true, true).slideDown(180, function () {
                $(this).find("input").first().trigger("focus");
            });
        });
    }

    /* 删东西之前先弹个框让人确认一下 */
    function bindConfirmAction() {
        $(document).on("click", "[data-confirm]", function (e) {
            var msg = $(this).data("confirm");
            if (msg && !window.confirm(msg)) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            }
        });
    }

    /* 密码框后面那个小眼睛，点一下能看到自己输的密码 */
    function bindPasswordToggle() {
        $(document).on("click", "[data-toggle-pwd]", function () {
            var $btn = $(this);
            var $input = $($btn.data("toggle-pwd"));
            if (!$input.length) return;
            var isPwd = $input.attr("type") === "password";
            $input.attr("type", isPwd ? "text" : "password");
            $btn.attr("title", isPwd ? "隐藏密码" : "显示密码");
            $btn.toggleClass("on", isPwd);
        });
    }

    /* 顶部栏，还有手机上点出来的那个抽屉菜单 */
    function bindLayout() {
        var $header = $(".lx-header");
        if ($header.length) {
            var onScroll = function () {
                $header.toggleClass("is-scrolled", $(window).scrollTop() > 8);
            };
            $(window).on("scroll", onScroll);
            onScroll();
        }

        $(document).on("click", "[data-drawer-open]", function () {
            $(".lx-drawer, .lx-drawer-mask").addClass("open");
            $("body").css("overflow", "hidden");
        });
        $(document).on("click", "[data-drawer-close], .lx-drawer-mask", function () {
            $(".lx-drawer, .lx-drawer-mask").removeClass("open");
            $("body").css("overflow", "");
        });

        // 后台左边那条侧栏
        $(document).on("click", "[data-admin-drawer-open]", function () {
            $(".lx-admin-side, .lx-admin-mask").addClass("open");
        });
        $(document).on("click", "[data-admin-drawer-close], .lx-admin-mask", function () {
            $(".lx-admin-side, .lx-admin-mask").removeClass("open");
        });
    }

    /* 页面往下滚的时候，元素一个个淡进来 */
    function bindReveal() {
        var items = document.querySelectorAll(".lx-reveal");
        if (!items.length) return;

        if (!("IntersectionObserver" in window)) {
            Array.prototype.forEach.call(items, function (el) { el.classList.add("in"); });
            return;
        }

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("in");
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.08, rootMargin: "0px 0px -40px 0px" });

        Array.prototype.forEach.call(items, function (el) { io.observe(el); });
    }

    /* 后台通用的那些操作，比如上下架、删除之类的 */
    var adminReload = function () { window.location.reload(); };

    function adminAction(opts) {
        return function (e) {
            e.preventDefault();
            var $el = $(this);
            if (opts.confirm && !window.confirm(opts.confirm)) return;

            var data = {};
            if (opts.id) data.id = $el.data(opts.id);
            if (opts.extra) {
                // 先在本行里找输入框，找不到再全局找，免得取错行
                var $scope = $el.closest("[data-scope]");
                $.each(opts.extra, function (k, v) {
                    var $field = $scope.length ? $scope.find(v) : $();
                    if (!$field.length) $field = $(v);
                    data[k] = $field.length ? $field.val() : $el.data(v);
                });
            }

            $el.prop("disabled", true);
            LX.post(opts.url, data)
                .done(function (res) {
                    if (res && res.ok) {
                        LX.toast(res.message || "操作成功", "success");
                        if (opts.reload !== false) window.setTimeout(adminReload, 600);
                    } else if (res && res.needLogin) {
                        LX.toast(res.message, "warning", "需要登录");
                        window.setTimeout(function () { window.location.href = "/Admin/Login"; }, 900);
                    } else {
                        LX.toast((res && res.message) || "操作失败", "error");
                    }
                })
                .fail(function () { LX.toast("网络异常，请稍后重试", "error"); })
                .always(function () { $el.prop("disabled", false); });
        };
    }

    function bindAdmin() {
        $(document).on("click", "[data-admin-ship]", adminAction({
            url: "/Admin/GoodsShipment", id: "adminShip", confirm: "确定要为这件商品发货吗？"
        }));
        $(document).on("click", "[data-admin-order-delete]", adminAction({
            url: "/Admin/Deleteorder", id: "adminOrderDelete",
            confirm: "确定取消这条订单明细吗？库存会一并回滚。"
        }));
        $(document).on("click", "[data-admin-user-auth]", adminAction({
            url: "/Admin/UserAuth", id: "adminUserAuth", confirm: "确定让该用户通过邮箱认证吗？"
        }));
        $(document).on("click", "[data-admin-user-reset]", adminAction({
            url: "/Admin/PwdInitialize", id: "adminUserReset",
            confirm: "确定把该用户的密码重置为 123456 吗？"
        }));
        $(document).on("click", "[data-admin-user-delete]", adminAction({
            url: "/Admin/UserDelete", id: "adminUserDelete",
            confirm: "删除后该用户数据不可恢复，确定继续吗？"
        }));
        $(document).on("click", "[data-admin-product-delete]", adminAction({
            url: "/Admin/ProductDelete", id: "adminProductDelete",
            confirm: "确定删除该商品吗？此操作不可恢复。"
        }));
        $(document).on("click", "[data-admin-product-toggle]", adminAction({
            url: "/Admin/ProductToggle", id: "adminProductToggle", reload: true
        }));
        $(document).on("click", "[data-admin-category-delete]", adminAction({
            url: "/Admin/CategoryDelete", id: "adminCategoryDelete",
            confirm: "确定删除该商品类别吗？"
        }));
        $(document).on("click", "[data-admin-category-rename]", adminAction({
            url: "/Admin/CategoryRename", id: "adminCategoryRename",
            extra: { name: "[data-category-name]" }, reload: true
        }));
        $(document).on("click", "[data-admin-category-create]", adminAction({
            url: "/Admin/CategoryCreate", extra: { name: "#newCategoryName" }, reload: true
        }));
    }

    /* 选完图片先在页面上预览一下 */
    function bindUploadPreview() {
        $(document).on("change", "[data-upload-preview]", function () {
            var $input = $(this);
            var target = $input.data("upload-preview");
            var name = $input.val().split("\\").pop();
            var $holder = $($input.data("upload-name"));
            if ($holder.length) $holder.text(name || "未选择文件");

            if (this.files && this.files[0]) {
                var reader = new FileReader();
                reader.onload = function (ev) {
                    $($(target)).html('<img src="' + ev.target.result + '" alt="预览" />');
                };
                reader.readAsDataURL(this.files[0]);
            }
        });
    }

    /* 页面加载完之后统一初始化一下 */
    $(function () {
        bindAddToCart();
        bindQtyStepper();
        bindCartPage();
        bindAddressPick();
        bindConfirmAction();
        bindPasswordToggle();
        bindLayout();
        bindReveal();
        bindAdmin();
        bindUploadPreview();

        // 后端带过来的提示消息，一加载就弹出来，弹完把节点删掉
        var $flash = $("#lxFlash");
        if ($flash.length) {
            var msg = $flash.data("message");
            var type = $flash.data("type") || "info";
            if (msg) LX.toast(msg, type);
            $flash.remove();
        }
    });

    window.LX = LX;
})(window, window.jQuery);
