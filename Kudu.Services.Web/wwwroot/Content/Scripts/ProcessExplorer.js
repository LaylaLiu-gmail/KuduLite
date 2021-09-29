var Utilities = (function () {
    function Utilities() {
    }
    Utilities.toRow = function (name, value) {
        var div = document.createElement("div");
        div.className = "row erow col-s-12";

        var namediv = document.createElement("div");
        namediv.className = "col-xs-4";
        var strong = document.createElement("strong");
        strong.textContent = name ? name.toString() : "NaN";
        namediv.appendChild(strong);

        var valuediv = document.createElement("div");
        valuediv.className = "col-xs-8";
        valuediv.textContent = typeof (value) !== "undefined" ? value.toString() : "NaN";

        div.appendChild(namediv);
        div.appendChild(valuediv);
        return div;
    };

    Utilities.errorDiv = function (value) {
        var div = document.createElement("div");
        div.className = "red-error";
        div.textContent = value;
        return div;
    };

    Utilities.makeDialog = function (jquery, height) {
        return jquery.dialog({
            autoOpen: false,
            width: "auto",
            height: height,
            buttons: {
                "Close": function () {
                    $(this).dialog("close");
                }
            }
        }).css("min-width", 600).css("max-width", 1000);
    };

    Utilities.makeArrayTable = function (id, headers, objects, attachedData) {
        if (typeof attachedData === "undefined") { attachedData = null; }
        var table = document.createElement("table");
        table.id = id;
        table.className = "table table-hover table-condensed";
        var tbody = document.createElement("tbody");
        var trHead = document.createElement("tr");
        for (var i = 0; i < headers.length; i++) {
            var thHead = document.createElement("th");
            thHead.textContent = headers[i];
            trHead.appendChild(thHead);
            tbody.appendChild(trHead);
        }

        for (var i = 0; i < objects.length; i++) {
            var cells = objects[i].tableCells();
            var row = document.createElement("tr");
            for (var j = 0; j < cells.length; j++) {
                var cell = document.createElement("td");
                if (cells[j] instanceof HTMLElement) {
                    cell.appendChild(cells[j]);
                } else {
                    cell.innerHTML = cells[j];
                }
                row.appendChild(cell);
            }
            if (attachedData !== null) {
                $(row).data(attachedData, objects[i]);
            }
            tbody.appendChild(row);
        }
        table.appendChild(tbody);
        return table;
    };

    Utilities.getArrayFromJson = function (jsonArray, action) {
        var array = [];
        for (var i = 0; i < jsonArray.length; i++) {
            array.push(action(jsonArray[i]));
        }
        return array;
    };

    Utilities.getArrayFromJsonObject = function (jsonObject, action) {
        var array = [];
        for (var propertyName in jsonObject) {
            array.push(action(propertyName, jsonObject[propertyName]));
        }
        return array;
    };

    Utilities.createDiv = function (id) {
        var div = document.createElement("div");
        div.id = id;
        return div;
    };

    Utilities.commaSeparateNumber = function (val) {
        var strVal = Math.floor(val).toString(10);
        while (/(\d+)(\d{3})/.test(strVal)) {
            strVal = strVal.replace(/(\d+)(\d{3})/, "$1" + "," + "$2");
        }
        return strVal;
    };

    Utilities.createTabs = function (baseId, tabsHeaders) {
        var tabs = Utilities.createDiv(baseId + "-tabs");

        var ul = document.createElement("ul");

        for (var i = 0; i < tabsHeaders.length; i++) {
            var tab = document.createElement("li");
            var anchor = document.createElement("a");
            anchor.setAttribute("href", "#" + baseId + "-" + tabsHeaders[i].toLowerCase().replace(" ", "-") + "-tab");
            anchor.textContent = tabsHeaders[i];
            tab.appendChild(anchor);
            ul.appendChild(tab);
        }
        tabs.appendChild(ul);
        return $(tabs);
    };

    Utilities.makeSimpleMenu = function (data) {
        var options = {
            selector: "tr",
            trigger: "right",
            callback: function (key) {
                var object = $(this).data(data);
                switch (key) {
                    case "properties":
                        object.dialog().dialog("open");
                        break;
                }
            },
            items: {
                "properties": { name: "Properties" }
            },
            events: {
                hide: function () {
                    $(this).removeClass("selectedMenu");
                },
                show: function () {
                    $(this).addClass("selectedMenu");
                }
            }
        };
        return options;
    };

    Utilities.downloadURL = function (url,showResponseMessage) {
        var hiddenIFrameID = "hiddenDownloader", iframe;
        iframe = document.getElementById(hiddenIFrameID);
        if (iframe === null) {
            iframe = document.createElement("iframe");
            iframe.id = hiddenIFrameID;
            iframe.style.display = "none";
            document.body.appendChild(iframe);
        }
        iframe.onload = function (e) {
            if (showResponseMessage) {
                var iframeDocument = iframe.contentDocument || iframe.contentWindow.document; // for both IE and other
                var iFrameBody = iframeDocument.getElementsByTagName('body')[0];
                var iframeText = $(iFrameBody).text();
                if (iframeText && iframeText.length > 0) {
                    showModal("Error", iframeText);
                }
            }
        }
        iframe.src = url;
    };

    Utilities.arrayToDivs = function (lines) {
        var htmls = [];
        var tmpDiv = jQuery(document.createElement("div"));
        for (var i = 0; i < lines.length; i++) {
            htmls.push(tmpDiv.text(lines[i]).html());
        }
        return htmls.join("<br />");
    };

    Utilities.ToTd = function (value) {
        var td = document.createElement('td');
        if (value instanceof HTMLElement) {
            td.appendChild(value);
        } else {
            td.textContent = value.toString();
        }
        return td;
    };

    Utilities.getButton = function (style, id, textContent, action, addStyle) {
        if (typeof addStyle === "undefined") { addStyle = true; }
        var button = document.createElement("button");
        button.className = style;
        button.id = id;
        button.textContent = textContent;

        $(button).button().click(function (e) {
            $(button).blur();
            action(e);
            $(button).blur();
        });
        if (addStyle) {
            $(button).css("margin-right", "20px");
        }
        return button;
    };


    Utilities.getCheckbox = function (id, textContent, isProfileRunning, iisIisProfileRunning) {
        var span = document.createElement("span");
        $(span).css("width", "138px");
        $(span).css("white-space", "nowrap");
        $(span).css("margin-right", "10px");

        var checkbox = document.createElement("input");
        checkbox.id = id;
        checkbox.type = "checkbox"
        $(checkbox).css("margin-right", "10px");
        $(checkbox).css("margin-left", "10px");

        checkbox.disabled = "";
        checkbox.checked = false;

        if (isProfileRunning)
        {
            checkbox.disabled = "disabled";
        }
        if (iisIisProfileRunning)
        {
            checkbox.checked = true;
        }

        span.appendChild(checkbox);

        var label = document.createElement("span");
        label.textContent = textContent;
        span.appendChild(label);

        return span;
    };

    return Utilities;
})();

var Instance = (function () {
    function Instance(json) {
        this._json = json;
    }

    Object.defineProperty(Instance.prototype, "Name", {
        get: function () {
            return this._json.name;
        },
        enumerable: false,
        configurable: true
    });

    Object.defineProperty(Instance.prototype, "IpAddress", {
        get: function () {
            return this._json.ipAddress;
        },
        enumerable: false,
        configurable: true
    });

    Object.defineProperty(Instance.prototype, "Status", {
        get: function () {
            return this._json.status;
        },
        enumerable: false,
        configurable: true
    });

    Object.defineProperty(Instance.prototype, "HostIpAddress", {
        get: function () {
            return this._json.hostIpAddress;
        },
        enumerable: false,
        configurable: true
    });

    Object.defineProperty(Instance.prototype, "NodeName", {
        get: function () {
            return this._json.nodeName;
        },
        enumerable: false,
        configurable: true
    });

    Object.defineProperty(Instance.prototype, "StartTime", {
        get: function () {
            return this._json.startTime;
        },
        enumerable: false,
        configurable: true
    });

    Instance.prototype.tableRow = function () {
        var tr = document.createElement('tr');
        tr.className = 'collapsable hoverable';

        tr.appendChild(Utilities.ToTd(this.Name));
        tr.appendChild(Utilities.ToTd(this.IpAddress));
        tr.appendChild(Utilities.ToTd(this.Status));
        tr.appendChild(Utilities.ToTd(this.HostIpAddress));
        tr.appendChild(Utilities.ToTd(this.NodeName));
        tr.appendChild(Utilities.ToTd(this.StartTime));

        var actions = [];
        var viewProcess = document.createElement('a');
        viewProcess.textContent = "View Processes"
        viewProcess.href = `javascript:processExplorerSetupAsync('${this.Name}');`;
        actions.push(viewProcess);

        var ssh = document.createElement('a');
        ssh.textContent = "SSH";
        ssh.href = `/instances/${this.Name}/webssh/host`;
        actions.push(ssh);

        var restartPod = document.createElement('a');
        restartPod.textContent = "Restart";
        restartPod.href = `javascript:restartPodAsync('${this.Name}');`;
        actions.push(restartPod);

        var actionCol = document.createElement('ul');
        actionCol.style = "list-style-type: none; padding-left:0;";
        for (var i = 0; i < actions.length; i++) {
            var col = document.createElement('li');
            col.appendChild(actions[i]);

            actionCol.appendChild(col);
        }

        tr.appendChild(Utilities.ToTd(actionCol));
        return $(tr);
    };

    return Instance;
})();

var Process = (function () {
    function Process(json) {
        this._json = json;
        this._json.href = `api/processes/${json.pid}`;
    }

    Object.defineProperty(Process.prototype, "Id", {
        get: function () {
            return this._json.pid;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "User", {
        get: function () {
            return this._json.user;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "PID", {
        get: function () {
            return this._json.pid;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "CPU", {
        get: function () {
            return this._json.cpu;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "Memory", {
        get: function () {
            return this._json.memory;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "TTY", {
        get: function () {
            return this._json.tty;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "Start", {
        get: function () {
            return this._json.start;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "Time", {
        get: function () {
            return this._json.time;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Process.prototype, "Command", {
        get: function () {
            return this._json.command;
        },
        enumerable: true,
        configurable: true
    });

    Process.prototype.tableRow = function (level) {
        var _this = this;
        var tr = document.createElement('tr');
        tr.setAttribute('data-depth', level.toString());
        tr.className = 'collapsable hoverable refersh';

        tr.appendChild(Utilities.ToTd(this.User));
        tr.appendChild(Utilities.ToTd(this.PID));
        tr.appendChild(Utilities.ToTd(this.CPU));
        tr.appendChild(Utilities.ToTd(this.Memory));
        tr.appendChild(Utilities.ToTd(this.TTY));
        tr.appendChild(Utilities.ToTd(this.Start));
        tr.appendChild(Utilities.ToTd(this.Time));
        tr.appendChild(Utilities.ToTd(this.Command));

        return $(tr);
    };

    Process.prototype.dialog = function () {
        if ($("#" + this._json.id.toString()).length > 0) {
            return $("#" + this._json.id.toString()).tabs("option", "active", 0);
        }

        var div = Utilities.createDiv(this._json.id.toString());
        div.setAttribute("title", this.FullName + ":" + this._json.id + " Properties");

        this.getProcessDatailsTabsHeaders().appendTo(div);

        this.getInfoTab().appendTo(div);
        this.getModulesTab().appendTo(div);
        this.getOpenHandlesTab().appendTo(div);
        this.getThreadsTab().appendTo(div);
        this.getEnvironmentVariablesTab().appendTo(div);

        return Utilities.makeDialog($(div).tabs(), 800);
    };

    Object.defineProperty(Process.prototype, "FullName", {
        get: function () {
            return (this._json.file_name === "N/A" ? this._json.name : this._json.file_name.split("\\").pop());
        },
        enumerable: true,
        configurable: true
    });

    Process.prototype.getOpenHandlesTab = function () {
        var div = Utilities.createDiv(this._json.id.toString() + "-handles-tab");
        var table = Utilities.makeArrayTable(div.id + "-table", ["Handles"], this._json.open_file_handles);
        div.appendChild(table);
        return $(div).hide();
    };

    Process.prototype.getThreadsTab = function () {
        var div = Utilities.createDiv(this._json.id.toString() + "-threads-tab");

        var table = Utilities.makeArrayTable(div.id + "-table", ["Id", "State", "More"], this._json.threads, "thread");
        div.appendChild(table);

        $(table).contextMenu(Utilities.makeSimpleMenu("thread"));

        return $(div).hide();
    };

    Process.prototype.getInfoTab = function () {
        var _this = this;
        var div = Utilities.createDiv(this._json.id.toString() + "-general-tab");

        div.appendChild(Utilities.toRow("id", this._json.id));
        div.appendChild(Utilities.toRow("name", this._json.name));
        div.appendChild(Utilities.toRow("file name", this._json.file_name));
        div.appendChild(Utilities.toRow("command line", this._json.command_line));
        div.appendChild(Utilities.toRow("description", this._json.description ? this._json.description : ""));
        div.appendChild(Utilities.toRow("user name", this._json.user_name));
        div.appendChild(Utilities.toRow("is scm site", this._json.is_scm_site));
        div.appendChild(Utilities.toRow("is webjob", this._json.is_scm_site));
        div.appendChild(Utilities.toRow("handle count", Utilities.commaSeparateNumber(this._json.handle_count)));
        div.appendChild(Utilities.toRow("module countid", Utilities.commaSeparateNumber(this._json.module_count)));
        div.appendChild(Utilities.toRow("thread count", Utilities.commaSeparateNumber(this._json.thread_count)));
        div.appendChild(Utilities.toRow("start time", this._json.start_time));
        div.appendChild(Utilities.toRow("total cpu time", this._json.total_cpu_time));
        div.appendChild(Utilities.toRow("user cpu time", this._json.user_cpu_time));
        div.appendChild(Utilities.toRow("privileged cpu time", this._json.privileged_cpu_time));
        div.appendChild(Utilities.toRow("working set", Utilities.commaSeparateNumber(this._json.working_set / 1024) + " KB"));
        div.appendChild(Utilities.toRow("peak working set", Utilities.commaSeparateNumber(this._json.peak_working_set / 1024) + " KB"));
        div.appendChild(Utilities.toRow("private memory", Utilities.commaSeparateNumber(this._json.private_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("virtual memory", Utilities.commaSeparateNumber(this._json.virtual_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("peak virtual memory", Utilities.commaSeparateNumber(this._json.peak_virtual_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("paged system memory", Utilities.commaSeparateNumber(this._json.paged_system_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("non-paged system memory", Utilities.commaSeparateNumber(this._json.non_paged_system_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("paged memory", Utilities.commaSeparateNumber(this._json.paged_memory / 1024) + " KB"));
        div.appendChild(Utilities.toRow("peak paged memory", Utilities.commaSeparateNumber(this._json.peak_paged_memory / 1024) + " KB"));

        var buttonDiv = document.createElement("div");
        buttonDiv.className = "buttons-row col-xs-12";

        buttonDiv.appendChild(Utilities.getButton("ui-button-danger", div.id + "-kill", "Kill", function () {
            _this.HTMLElement.removeClass("hoverable");
            _this.HTMLElement.addClass("dying");
            _this.kill().done(function () {
                processExplorerSetupAsync();
                _this.dialog().dialog("close");
            });
        }));

        buttonDiv.appendChild(Utilities.getButton("ui-button-info", div.id + "-dumb", "Download memory dump", function () {
            Utilities.downloadURL(_this._json.minidump);
        }));

        div.appendChild(buttonDiv);

        return $(div).hide();
    };

    Process.prototype.getModulesTab = function () {
        var div = document.createElement("div");
        div.id = this._json.id.toString() + "-modules-tab";

        var table = Utilities.makeArrayTable(div.id + "-table", ["File Name", "File Version", "More"], this._json.modules, "module");
        div.appendChild(table);
        $(table).contextMenu(Utilities.makeSimpleMenu("module"));

        return $(div).hide();
    };

    Process.prototype.getEnvironmentVariablesTab = function () {
        var _this = this;
        var div = Utilities.createDiv(this._json.id.toString() + "-environment-variables-tab");

        var table = Utilities.makeArrayTable(div.id + "-table", ["Key", "Value"], this._json.environment_variables);
        div.appendChild(table);
        return $(div).hide();
    };

    Process.prototype.getProcessDatailsTabsHeaders = function () {
        return Utilities.createTabs(this._json.id.toString(), ["General", "Modules", "Handles", "Threads", "Environment Variables"]);
    };

    Process.prototype.kill = function () {
        var instanceName = $("#instanceName").text();
        return $.ajax({
            url: `${this._json.href}?instanceName=${instanceName}`,
            type: "DELETE"
        });
    };

    Process.getIdFromHref = function (href) {
        return parseInt(href.substr(href.lastIndexOf("/") + 1));
    };
    return Process;
})();

var Tree = (function () {
    function Tree() {
        this.roots = [];
    }
    Tree.prototype.contains = function (pid) {
        for (var i = 0; i < this.roots.length; i++) {
            if (Tree.recursiveContains(this.roots[i], pid)) {
                return true;
            }
        }
        return false;
    };

    Tree.prototype.buildTree = function (nodeList) {
        nodeList.sort(function (a, b) {
            return a.process.Id - b.process.Id;
        });
        this.roots.sort(function (a, b) {
            return a.process.Id - b.process.Id;
        });
        for (var i = 0; i < this.roots.length; i++) {
            Tree.addChildren(this.roots[i], nodeList);
        }
        //$(".collapsable").remove();
        //$(".expandable").remove();
        for (var i = 0; i < this.roots.length; i++) {
            Tree.printTreeTable(this.roots[i], 0, $("#proctable"));
        }
    };

    Tree.recursiveContains = function (node, pid) {
        if (node.process.Id === pid) {
            return true;
        } else {
            for (var i = 0; i < node.children.length; i++) {
                if (Tree.recursiveContains(node[i], pid)) {
                    return true;
                }
            }
        }
        return false;
    };

    Tree.addChildren = function (node, nodeList) {
        for (var i = 0; i < nodeList.length; i++) {
            node.children.push(nodeList[i]);
        }
    };

    Tree.printTreeTable = function (node, level, tableRoot) {
        var jcurrent = node.process.tableRow(level);
        jcurrent.data("proc", node.process).appendTo(tableRoot);
        node.process.HTMLElement = jcurrent;
        for (var i = 0; i < node.children.length; i++) {
            if (node.children[i].process.Id !== node.process.Id) {
                Tree.printTreeTable(node.children[i], level + 1, tableRoot);
            }
        }
    };

    //debug method leave it
    Tree.printTreeUl = function (node, parent) {
        var current = "<li><span>";
        current += "(" + node.process.Id + ") " + node.process.Name;
        current += "</span></li>";
        var jcurrent = $(current).appendTo(parent);
        if (node.children.length > 0) {
            jcurrent = $("<ul></ul>").appendTo(jcurrent);
            for (var i = 0; i < node.children.length; i++) {
                Tree.printTreeUl(node.children[i], jcurrent);
            }
        }
    };

    //debug method leave it
    Tree.printTreeConsole = function (node, level) {
        var indentation = "";
        for (var i = 0; i < level - 1; i++) {
            indentation += "    ";
        }
        if (indentation.length != 0 || level > 0)
            indentation += "|__>";
        console.log(indentation + "(" + node.process.Id + ") " + node.process.Name);
        for (var i = 0; i < node.children.length; i++) {
            Tree.printTreeConsole(node.children[i], level + 1);
        }
    };
    return Tree;
})();

var ProcessNode = (function () {
    function ProcessNode(process) {
        this.process = process;
        this.children = [];
    }
    return ProcessNode;
})();

var nodeList;

function processExplorerSetupAsync(instanceName) {
    var processTree = new Tree();
    nodeList = [];
    $(".refersh").remove();

    if (instanceName) {
        $("#instanceName").text(instanceName);
    }
    else {
        // refresh
        $("#proc-loading").show();
        instanceName = $("#instanceName").text();
    }

    $.getJSON(`api/processes?instanceName=${instanceName}`, function (data) {
        for (var i = 0; i < data.length; i++) {
            var p = new Process(data[i]);
            var processNode = new ProcessNode(p);
            if (i === 0) {
                processTree.roots.push(processNode);
            }

            nodeList.push(new ProcessNode(p));
        }

        processTree.buildTree(nodeList);
        $("#proc-loading").hide();
    });
}

function getInstancesAsync() {
    $("#proc-loading").show();
    $.getJSON("api/instances", function (data) {
        var defaultInstanceName = "";
        for (var i = 0; i < data.length; i++) {
            var instance = new Instance(data[i]);
            if (i === 0) {
                defaultInstanceName = instance.Name;
            }

            var jcurrent = instance.tableRow();
            jcurrent.appendTo($("#instancetable"));
        }

        processExplorerSetupAsync(defaultInstanceName);
    });
}

function restartPodAsync(podName) {
    $.ajax({
        url: `api/instances/${podName}/restart`,
        type: "PUT"
    }).always(() => {
        location.reload();
    });
}

function enableCollabsableNodes() {
    //http://stackoverflow.com/questions/5636375/how-to-create-a-collapsing-tree-table-in-html-css-js
    $("#proctable").on("click", ".toggle", function (e) {
        e.preventDefault();
        e.stopPropagation();

        //Gets all <tr>"s  of greater depth
        //below element in the table
        //TODO: change back to tr if there is an issue
        var findChildren = function (_tr) {
            var depth = _tr.data("depth");
            return _tr.nextUntil($("tr").filter(function () {
                return $(this).data("depth") <= depth;
            }));
        };

        var el = $(this);
        var tr = el.closest("tr");
        var children = findChildren(tr);

        //Remove already collapsed nodes from children so that we don"t
        //make them visible.
        var subnodes = children.filter(".expandable");
        subnodes.each(function () {
            var subnode = $(this);
            var subnodeChildren = findChildren(subnode);
            children = children.not(subnodeChildren);
        });

        //Change icon and hide/show children
        if (tr.hasClass("collapsable")) {
            tr.removeClass("collapsable").addClass("expandable");
            children.hide();
        } else {
            tr.removeClass("expandable").addClass("collapsable");
            children.show();
        }
        return children;
    });
}

function overrideRightClickMenu() {
    var options = {
        selector: "tr",
        trigger: "right",
        callback: function (key) {
            var process = $(this).data("proc");
            switch (key) {
                case "kill":
                    $(this).removeClass("hoverable");
                    $(this).addClass("dying");
                    process.kill().done(function () {
                        return processExplorerSetupAsync(null);
                    }).fail(function () {
                        return processExplorerSetupAsync(null);
                    });
                    break;
                case "dump1":
                    Utilities.downloadURL(process.Minidump + "?dumpType=1");
                    break;
                case "dump2":
                    Utilities.downloadURL(process.Minidump + "?dumpType=2");
                    break;
                case "properties":
                    process.dialog().dialog("open");
                    $("li")[0].click();
                    $("li").blur();
                    break;
            }
        },
        items: {
            "kill": { name: "Kill" },
            "dump": {
                name: "Download Memory Dump",
                "items": {
                    "dump1": { name: "Mini Dump" },
                    "dump2": { name: "Full Dump" }
                }
            },
            "sep1": "---------",
            "properties": { name: "Properties" }
        },
        events: {
            hide: function () {
                $(this).removeClass("selectedMenu");
            },
            show: function () {
                $(this).addClass("selectedMenu");
            }
        }
    };
    $("#proctable").contextMenu(options);
}

function searchForHandle() {
    var name = $("#name").val().toLowerCase();
    var result = [];
    for (var i = 0; i < nodeList.length; i++) {
        for (var j = 0; j < nodeList[i].process.FileHandles.length; j++) {
            var check = nodeList[i].process.FileHandles[j].file_name.replace(/\\+$/, "").toLowerCase();
            check = check.substring(check.lastIndexOf("\\"));
            if (check.indexOf(name) !== -1) {
                result.push(nodeList[i].process.FullName + ":" + nodeList[i].process.Id + " -> " + nodeList[i].process.FileHandles[j].file_name);
            }
        }
    }
    if (result.length > 0) {
        $("#handle-result").html(Utilities.arrayToDivs(result));
    } else {
        $("#handle-result").html(Utilities.errorDiv("No handle found").outerHTML);
    }
}

function showModal(title, content) {

    if (!$("#generalModal").hasClass('ui-dialog-content')) { //check if already init-ed
        $("#generalModal").dialog({
            autoOpen: false,
            resizable: false,
            draggable: false,
            modal: true,
            width: "500px",
            open: function () {
                $(this).siblings(".ui-dialog-titlebar")
                    .find("button").blur();
            },
            create: function () {
                $(".ui-dialog").find(".ui-dialog-titlebar").css({
                    'background-image': 'none',
                    'background-color': 'white',
                    'border': 'none'
                });
            }
        });
    }
    $('#generalModal').dialog('option', 'title', title);
    $("#generalModal .content").text(content);
    $("#generalModal").dialog("open");
}

window.onload = function () {
    //http://stackoverflow.com/questions/5518181/jquery-deferreds-when-and-the-fail-callback-arguments
    $.whenAll = function (firstParam) {
        var args = arguments, sliceDeferred = [].slice, i = 0, length = args.length, count = length, rejected, deferred = length <= 1 && firstParam && jQuery.isFunction(firstParam.promise) ? firstParam : jQuery.Deferred();

        function resolveFunc(i, reject) {
            return function (value) {
                rejected |= reject;
                args[i] = arguments.length > 1 ? sliceDeferred.call(arguments, 0) : value;
                if (!(--count)) {
                    // Strange bug in FF4:
                    // Values changed onto the arguments object sometimes end up as undefined values
                    // outside the $.when method. Cloning the object into a fresh array solves the issue
                    var fn = rejected ? deferred.rejectWith : deferred.resolveWith;
                    fn.call(deferred, deferred, sliceDeferred.call(args, 0));
                }
            };
        }

        if (length > 1) {
            for (; i < length; i++) {
                if (args[i] && jQuery.isFunction(args[i].promise)) {
                    args[i].promise().then(resolveFunc(i, false), resolveFunc(i, true));
                } else {
                    --count;
                }
            }
            if (!count) {
                deferred.resolveWith(deferred, args);
            }
        } else if (deferred !== firstParam) {
            deferred.resolveWith(deferred, length ? [firstParam] : []);
        }
        return deferred.promise();
    };

    $("#find-file-handle").button().click(function () {
        $("#dialog-form").dialog("open");
    });

    $("#dialog-form").dialog({
        autoOpen: false,
        height: 300,
        width: 800,
        buttons: {
            "Search": function () {
                return searchForHandle();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });

    $("#dialog-form").keypress(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            e.preventDefault();
            e.stopPropagation();
            searchForHandle();
        }
    });

    getInstancesAsync();
    enableCollabsableNodes();
    overrideRightClickMenu();
};